using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast
{
    public enum Sfx { Click, Pick, Place, Clear, Invalid, GameOver, Fanfare }

    /// <summary>
    /// Âm thanh của game. Mặc định tự tổng hợp âm (sine / triangle) bằng code;
    /// nếu có file trong Assets/Resources/BlockBlast/Audio (vd: "Clear.wav", "Music.mp3") thì dùng file đó.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        const string AudioFolder = "BlockBlast/Audio/";
        const string SoundKey = "BB_SOUND", MusicKey = "BB_MUSIC";
        const int SampleRate = 44100;

        static SoundManager instance;

        public static SoundManager Instance
        {
            get
            {
                if (instance == null) instance = new GameObject(nameof(SoundManager)).AddComponent<SoundManager>();
                return instance;
            }
        }

        AudioSource sfxSource, pitchedSource, musicSource;
        readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
        bool soundOn, musicOn;

        public bool SoundOn
        {
            get => soundOn;
            set
            {
                soundOn = value;
                PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
            }
        }

        public bool MusicOn
        {
            get => musicOn;
            set
            {
                musicOn = value;
                PlayerPrefs.SetInt(MusicKey, value ? 1 : 0);
                musicSource.mute = !value;
            }
        }

        void Awake()
        {
            sfxSource = AddSource();
            pitchedSource = AddSource();
            musicSource = AddSource();
            musicSource.loop = true;
            musicSource.volume = 0.35f;

            soundOn = PlayerPrefs.GetInt(SoundKey, 1) == 1;
            musicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;
            musicSource.mute = !musicOn;

            LoadClips();
            musicSource.clip = LoadOrSynth("Music", MusicLength, MusicWave);
        }

        public void PlayMusic()
        {
            if (!musicSource.isPlaying) musicSource.Play();
        }

        public void Play(Sfx sfx, float volume = 1f)
        {
            if (soundOn) sfxSource.PlayOneShot(clips[sfx], volume);
        }

        /// <summary>Âm phá hàng, cao dần theo combo.</summary>
        public void PlayClear(int combo)
        {
            if (!soundOn) return;
            pitchedSource.pitch = 1f + 0.07f * Mathf.Clamp(combo - 1, 0, 10);
            pitchedSource.PlayOneShot(clips[Sfx.Clear]);
        }

        AudioSource AddSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        // ---------------------------------------------------------------- tổng hợp âm thanh

        void LoadClips()
        {
            var noise = new System.Random(7);
            Register(Sfx.Click, 0.07f, t => Sine(880f, t) * Mathf.Exp(-t * 60f) * 0.5f);
            Register(Sfx.Pick, 0.09f, t => Sine(520f + 2500f * t, t) * Mathf.Exp(-t * 35f) * 0.45f);
            Register(Sfx.Place, 0.16f, t =>
                (Sine(180f - 300f * t, t) * 0.8f + ((float)noise.NextDouble() * 2f - 1f) * 0.25f * Mathf.Exp(-t * 80f))
                * Mathf.Exp(-t * 22f) * 0.7f);
            Register(Sfx.Clear, 0.5f, Arpeggio(new[] { 659.25f, 783.99f, 987.77f, 1318.5f }, 0.06f, 9f, 0.45f));
            Register(Sfx.Invalid, 0.15f, t => Mathf.Sign(Sine(150f, t)) * Mathf.Exp(-t * 18f) * 0.2f);
            Register(Sfx.GameOver, 1.1f, Arpeggio(new[] { 523.25f, 440f, 349.23f, 261.63f }, 0.2f, 5f, 0.45f));
            Register(Sfx.Fanfare, 0.9f,
                Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f }, 0.09f, 4f, 0.4f));
        }

        void Register(Sfx sfx, float duration, Func<float, float> wave) =>
            clips[sfx] = LoadOrSynth(sfx.ToString(), duration, wave);

        /// <summary>Dùng file âm thanh trong Resources nếu có, không thì tổng hợp bằng hàm sóng wave(t).</summary>
        static AudioClip LoadOrSynth(string name, float duration, Func<float, float> wave)
        {
            var custom = Resources.Load<AudioClip>(AudioFolder + name);
            return custom != null ? custom : Synth(name, duration, wave);
        }

        // Nhạc nền: vòng hợp âm C - Am - F - G, rải nốt móc đơn + bass nhẹ, lặp 8 giây.
        static readonly float[][] Chords =
        {
            new[] { 261.63f, 329.63f, 392.00f },
            new[] { 220.00f, 261.63f, 329.63f },
            new[] { 174.61f, 220.00f, 261.63f },
            new[] { 196.00f, 246.94f, 293.66f },
        };

        // nốt thứ mấy trong hợp âm được rải, 3 = nốt gốc lên quãng 8
        static readonly int[] MelodyPattern = { 0, 1, 2, 3, 2, 1, 2, 1 };
        const float ChordLength = 2f, NoteLength = 0.25f;
        static float MusicLength => ChordLength * Chords.Length;

        static float MusicWave(float t)
        {
            int chordIndex = Mathf.Min(Chords.Length - 1, (int)(t / ChordLength));
            float[] chord = Chords[chordIndex];
            float chordTime = t - chordIndex * ChordLength;

            int noteIndex = Mathf.Min(MelodyPattern.Length - 1, (int)(chordTime / NoteLength));
            float noteTime = chordTime - noteIndex * NoteLength;
            int note = MelodyPattern[noteIndex];
            float freq = note == 3 ? chord[0] * 2f : chord[note];

            float lead = Sine(freq * 2f, t) * Mathf.Exp(-noteTime * 6f) * 0.16f * Mathf.Min(1f, noteTime * 100f);
            float bassEnvelope = Mathf.Min(1f, chordTime * 8f) * Mathf.Min(1f, (ChordLength - chordTime) * 8f);
            float bass = (Sine(chord[0] * 0.5f, t) * 0.8f + Sine(chord[0], t) * 0.2f) * 0.2f * bassEnvelope;
            return lead + bass;
        }

        /// <summary>Rải lần lượt các nốt freqs, mỗi nốt dài step giây và tắt dần theo decay.</summary>
        static Func<float, float> Arpeggio(float[] freqs, float step, float decay, float volume)
        {
            return t =>
            {
                int i = Mathf.Min(freqs.Length - 1, (int)(t / step));
                float noteTime = t - i * step;
                float wave = Triangle(freqs[i], t) * 0.6f + Sine(freqs[i] * 2f, t) * 0.2f;
                return wave * Mathf.Exp(-noteTime * decay) * volume * Mathf.Min(1f, noteTime * 200f);
            };
        }

        static AudioClip Synth(string name, float duration, Func<float, float> wave)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(wave(i / (float)SampleRate), -1f, 1f);

            int fade = Mathf.Min(n, 400); // giảm dần ở cuối để tránh tiếng "tách"
            for (int i = 0; i < fade; i++) data[n - 1 - i] *= i / (float)fade;

            var clip = AudioClip.Create(name, n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sine(float freq, float t) => Mathf.Sin(2f * Mathf.PI * freq * t);

        static float Triangle(float freq, float t)
        {
            float x = freq * t;
            return 4f * Mathf.Abs(x - Mathf.Floor(x + 0.5f)) - 1f;
        }
    }
}

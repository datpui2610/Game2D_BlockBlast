using System;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>Các popup SETTINGS / PAUSED / GAME OVER, hiển thị trên một lớp phủ toàn màn hình.</summary>
    public class Popups : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        public static Popups Create(Transform parent)
        {
            var layer = UIKit.MakeRect("Popups", parent, Vector2.zero, Vector2.zero);
            UIKit.StretchFull(layer);
            return layer.gameObject.AddComponent<Popups>();
        }

        public void Close()
        {
            IsOpen = false;
            UIKit.DestroyChildren(transform);
        }

        public void ShowSettings()
        {
            var panel = Open("SETTINGS", new Vector2(760, 640));
            AddSoundToggles(panel, 130f);
            UIKit.MakeButton("Close", panel, Theme.Green, new Vector2(440, 140), new Vector2(0, -200), "OK", 62, Close);
        }

        public void ShowPause(Action onRestart, Action onHome)
        {
            var panel = Open("PAUSED", new Vector2(760, 980));
            AddSoundToggles(panel, 300f);

            var size = new Vector2(520, 140);
            UIKit.MakeButton("Resume", panel, Theme.Green, size, new Vector2(0, -40), "RESUME", 62, Close);
            UIKit.MakeButton("Restart", panel, Theme.Orange, size, new Vector2(0, -210), "RESTART", 62, onRestart);
            UIKit.MakeButton("Home", panel, Theme.Blue, size, new Vector2(0, -380), "HOME", 62, onHome);
        }

        public void ShowGameOver(int score, int best, bool isNewBest, Action onHome, Action onReplay)
        {
            var panel = Open("GAME OVER", new Vector2(780, 860));

            var box = UIKit.MakeImage("ScoreBox", panel, SpriteFactory.RoundedRect(30), Theme.Navy,
                new Vector2(560, 300), new Vector2(0, 170), true);
            UIKit.MakeText("Label", box.transform, "SCORE", 48, Theme.PaleBlue, new Vector2(500, 60),
                new Vector2(0, 95));
            var scoreText = UIKit.MakeText("Score", box.transform, "0", 132, Color.white, new Vector2(540, 160),
                new Vector2(0, -20), TextAnchor.MiddleCenter, 5f);
            // điểm đếm dần từ 0 lên
            Tweener.Run(scoreText, 0.8f, t => scoreText.text = Mathf.RoundToInt(score * Ease.OutCubic(t)).ToString(),
                null, 0.25f);

            UIKit.MakeText("Best", panel, "BEST: " + best, 54, Theme.Gold, new Vector2(600, 80), new Vector2(0, -50));
            if (isNewBest)
            {
                var newBest = UIKit.MakeText("NewBest", panel, "NEW BEST!", 70, Theme.Pink, new Vector2(600, 100),
                    new Vector2(0, -140), TextAnchor.MiddleCenter, 5f);
                Pulse.Attach(newBest, 0.08f, 6f);
                SoundManager.Instance.Play(Sfx.Fanfare);
            }

            var size = new Vector2(320, 140);
            UIKit.MakeButton("Home", panel, Theme.Blue, size, new Vector2(-180, -310), "HOME", 58, onHome);
            UIKit.MakeButton("Replay", panel, Theme.Orange, size, new Vector2(180, -310), "REPLAY", 58, onReplay);
        }

        /// <summary>Mở popup mới (đóng popup cũ nếu có), trả về panel để thêm nội dung vào.</summary>
        RectTransform Open(string title, Vector2 size)
        {
            Close();
            IsOpen = true;

            // lớp phủ tối, đồng thời chặn thao tác với phần phía sau
            var overlay = UIKit.MakeImage("Overlay", transform, null, Color.clear, Vector2.zero, Vector2.zero);
            UIKit.StretchFull(overlay.rectTransform);
            overlay.raycastTarget = true;

            var frame = UIKit.MakeImage("Frame", overlay.transform, SpriteFactory.RoundedRect(44), Theme.PopupFrame,
                size + new Vector2(24, 24), Vector2.zero, true);
            var panel = UIKit.MakeImage("Panel", frame.transform, SpriteFactory.RoundedRect(36), Theme.PopupPanel,
                size, Vector2.zero, true);
            var ribbon = UIKit.MakeImage("Ribbon", frame.transform, SpriteFactory.Button(), Theme.Blue,
                new Vector2(size.x * 0.72f, 130), new Vector2(0, size.y / 2f + 6f), true);
            UIKit.MakeText("Title", ribbon.transform, title, 68, Color.white, new Vector2(size.x, 130),
                new Vector2(0, 6), TextAnchor.MiddleCenter, 4f);

            // nền tối dần, khung bật nảy
            var frameRect = frame.rectTransform;
            Tweener.Run(overlay, 0.28f, t =>
            {
                overlay.color = new Color(0, 0, 0, 0.65f * t);
                frameRect.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, Ease.OutBack(t));
            });
            return panel.rectTransform;
        }

        static void AddSoundToggles(RectTransform panel, float y)
        {
            var sound = SoundManager.Instance;
            AddToggle(panel, "SOUND", y, () => sound.SoundOn, on => sound.SoundOn = on);
            AddToggle(panel, "MUSIC", y - 150f, () => sound.MusicOn, on => sound.MusicOn = on);
        }

        /// <summary>Một dòng gồm nhãn và nút ON / OFF.</summary>
        static void AddToggle(RectTransform panel, string label, float y, Func<bool> get, Action<bool> set)
        {
            UIKit.MakeText(label, panel, label, 58, Color.white, new Vector2(320, 90), new Vector2(-150, y),
                TextAnchor.MiddleLeft);
            var button = UIKit.MakeButton(label + "Toggle", panel, Theme.Grey, new Vector2(230, 110),
                new Vector2(180, y), "OFF", 52, null);
            var text = button.GetComponentInChildren<Text>();

            void Refresh()
            {
                bool on = get();
                UIKit.SetButtonColor(button, on ? Theme.Green : Theme.Grey);
                text.text = on ? "ON" : "OFF";
            }

            button.onClick.AddListener(() =>
            {
                set(!get());
                Refresh();
            });
            Refresh();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BlockBlast
{
    /// <summary>Màn hình chính: logo, điểm cao nhất, nút PLAY / SETTINGS và các khối trang trí trôi nổi.</summary>
    public class HomeScreen : MonoBehaviour
    {
        const int DecoCount = 16;
        const float DecoEdgeY = 1050f; // khối trang trí trôi quá mép trên thì quay lại từ mép dưới

        readonly List<(RectTransform Rect, Vector2 BasePos)> titleTiles = new List<(RectTransform, Vector2)>();
        readonly List<(RectTransform Rect, float Speed, float Spin)> decos = new List<(RectTransform, float, float)>();
        Text bestText;

        public static HomeScreen Create(Transform parent, Vector2 size, Skin skin, Action onPlay, Action onSettings)
        {
            var home = UIKit.MakeRect("HomeScreen", parent, size, Vector2.zero).gameObject.AddComponent<HomeScreen>();
            home.Build(skin, onPlay, onSettings);
            return home;
        }

        public void SetBest(int best) => bestText.text = best.ToString();

        void Build(Skin skin, Action onPlay, Action onSettings)
        {
            AddDecoBlocks(skin.Blocks);

            if (skin.Logo != null)
            {
                var logo = UIKit.MakeImage("Logo", transform, skin.Logo, Color.white, new Vector2(900, 600),
                    new Vector2(0, 450));
                logo.preserveAspect = true;
                titleTiles.Add((logo.rectTransform, logo.rectTransform.anchoredPosition));
            }
            else
            {
                AddTitleWord("BLOCK", 560, skin.Blocks, 0);
                AddTitleWord("BLAST", 380, skin.Blocks, 2);
            }

            var bestBox = UIKit.MakeImage("BestBox", transform, SpriteFactory.RoundedRect(30),
                new Color(0, 0, 0, 0.28f), new Vector2(520, 170), new Vector2(0, 90), true);
            UIKit.MakeText("BestLabel", bestBox.transform, "BEST SCORE", 40, Theme.Gold, new Vector2(500, 60),
                new Vector2(0, 40));
            bestText = UIKit.MakeText("Best", bestBox.transform, "0", 76, Color.white, new Vector2(500, 90),
                new Vector2(0, -25));

            var play = UIKit.MakeButton("Play", transform, Theme.Green, new Vector2(560, 190), new Vector2(0, -250),
                "PLAY", 96, onPlay);
            Pulse.Attach(play, 0.04f, 4f);

            UIKit.MakeButton("Settings", transform, Theme.Blue, new Vector2(440, 130), new Vector2(0, -480),
                "SETTINGS", 56, onSettings);

            UIKit.MakeText("Hint", transform, "Kéo khối vào bảng 8x8.\nXếp đầy hàng hoặc cột để phá khối!", 40,
                new Color(1, 1, 1, 0.75f), new Vector2(1000, 120), new Vector2(0, -760), TextAnchor.MiddleCenter, 2f);
        }

        void AddDecoBlocks(Sprite[] blockSprites)
        {
            for (int i = 0; i < DecoCount; i++)
            {
                float size = Random.Range(50f, 110f);
                var deco = UIKit.MakeImage("Deco", transform, blockSprites[i % blockSprites.Length],
                    new Color(1, 1, 1, 0.16f), new Vector2(size, size),
                    new Vector2(Random.Range(-500f, 500f), Random.Range(-1000f, 1000f)));
                deco.rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                decos.Add((deco.rectTransform, Random.Range(40f, 110f), Random.Range(-30f, 30f)));
            }
        }

        /// <summary>Ghép một chữ bằng các khối màu, mỗi khối một chữ cái.</summary>
        void AddTitleWord(string word, float y, Sprite[] blockSprites, int colorOffset)
        {
            const float spacing = 168f;
            float x0 = -(word.Length - 1) * spacing / 2f;
            for (int i = 0; i < word.Length; i++)
            {
                var tile = UIKit.MakeImage("Letter_" + word[i], transform,
                    blockSprites[(i + colorOffset) % blockSprites.Length], Color.white,
                    new Vector2(156, 156), new Vector2(x0 + i * spacing, y));
                UIKit.MakeText("Letter", tile.transform, word[i].ToString(), 112, Color.white, new Vector2(156, 156),
                    new Vector2(0, 2), TextAnchor.MiddleCenter, 5f);
                titleTiles.Add((tile.rectTransform, tile.rectTransform.anchoredPosition));
            }
        }

        void Update()
        {
            float time = Time.unscaledTime, dt = Time.unscaledDeltaTime;

            // các chữ cái nhấp nhô như sóng
            for (int i = 0; i < titleTiles.Count; i++)
            {
                var (rect, basePos) = titleTiles[i];
                rect.anchoredPosition = basePos + new Vector2(0, Mathf.Sin(time * 3f + i * 0.55f) * 10f);
            }

            // khối trang trí vừa trôi lên vừa xoay
            foreach (var (rect, speed, spin) in decos)
            {
                var pos = rect.anchoredPosition + new Vector2(0, speed * dt);
                if (pos.y > DecoEdgeY) pos = new Vector2(Random.Range(-500f, 500f), -DecoEdgeY);
                rect.anchoredPosition = pos;
                rect.Rotate(0, 0, spin * dt);
            }
        }
    }
}

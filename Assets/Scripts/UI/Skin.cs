using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Bộ hình ảnh của game. Mỗi hình được lấy theo thứ tự ưu tiên:
    /// sprite kéo vào Inspector → file trong Assets/Resources/BlockBlast → hình vẽ bằng code.
    /// </summary>
    public class Skin
    {
        const string ResourceFolder = "BlockBlast/";

        public Sprite[] Blocks { get; }
        public Sprite GreyBlock { get; }

        /// <summary>Có thể null: khi đó game dùng nền chuyển màu vẽ bằng code.</summary>
        public Sprite Background { get; }

        /// <summary>Có thể null: khi đó màn hình chính ghép chữ BLOCK BLAST bằng các khối.</summary>
        public Sprite Logo { get; }

        /// <summary>Icon bánh răng của nút tạm dừng. Có thể null: khi đó nút vẽ hai vạch dọc.</summary>
        public Sprite SettingsIcon { get; }

        readonly Dictionary<Color, Sprite> buttons = new Dictionary<Color, Sprite>();

        public Skin(Sprite[] customBlocks, Sprite customBackground, Sprite customLogo)
        {
            Blocks = new Sprite[Theme.BlockColors.Length];
            for (int i = 0; i < Blocks.Length; i++)
            {
                var custom = customBlocks != null && i < customBlocks.Length ? customBlocks[i] : null;
                var color = Theme.BlockColors[i];
                Blocks[i] = Resolve(custom, "block_" + i, () => SpriteFactory.Block(color));
            }
            GreyBlock = Resolve(null, "block_grey", () => SpriteFactory.Block(Theme.Grey));
            Background = Resolve(customBackground, "background");
            Logo = Resolve(customLogo, "logo");
            SettingsIcon = Resolve(null, "icon_settings");

            buttons[Theme.Green] = Resolve(null, "button_green");
            buttons[Theme.Blue] = Resolve(null, "button_blue");
            buttons[Theme.Orange] = Resolve(null, "button_yellow");
            buttons[Theme.Grey] = Resolve(null, "button_grey");
        }

        /// <summary>Ảnh nút ứng với màu trong Theme; null nếu không có ảnh (nút sẽ được vẽ bằng code).</summary>
        public Sprite Button(Color color) => buttons.TryGetValue(color, out var sprite) ? sprite : null;

        static Sprite Resolve(Sprite custom, string resourceName, Func<Sprite> fallback = null)
        {
            if (custom != null) return custom;
            var loaded = LoadResourceSprite(ResourceFolder + resourceName);
            if (loaded != null) return loaded;
            return fallback?.Invoke();
        }

        /// <summary>Đọc ảnh trong Assets/Resources (dù được import dạng Sprite hay Texture).</summary>
        static Sprite LoadResourceSprite(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return sprite;
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}

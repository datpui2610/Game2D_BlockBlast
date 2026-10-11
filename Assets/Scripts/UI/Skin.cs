using System;
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
        public Sprite Background { get; }

        /// <summary>Có thể null: khi đó màn hình chính ghép chữ BLOCK BLAST bằng các khối.</summary>
        public Sprite Logo { get; }

        public Skin(Sprite[] customBlocks, Sprite customBackground, Sprite customLogo)
        {
            Blocks = new Sprite[Theme.BlockColors.Length];
            for (int i = 0; i < Blocks.Length; i++)
            {
                var custom = customBlocks != null && i < customBlocks.Length ? customBlocks[i] : null;
                var color = Theme.BlockColors[i];
                Blocks[i] = Resolve(custom, "block_" + i, () => SpriteFactory.Block(color));
            }
            GreyBlock = SpriteFactory.Block(Theme.Grey);
            Background = Resolve(customBackground, "background",
                () => SpriteFactory.Gradient(Theme.BackgroundTop, Theme.BackgroundBottom));
            Logo = Resolve(customLogo, "logo");
        }

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

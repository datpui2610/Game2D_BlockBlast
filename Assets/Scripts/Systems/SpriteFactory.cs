using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Vẽ hình ảnh của game bằng code (khối, nút, panel, nền...) để project chạy được ngay
    /// mà không cần import texture. Mỗi sprite chỉ vẽ một lần rồi được lưu cache.
    /// </summary>
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Hình chữ nhật bo góc màu trắng (dùng Image.Type.Sliced, tô màu bằng Image.color).</summary>
        public static Sprite RoundedRect(int radius)
        {
            int size = radius * 2 + 4;
            return GetOrDraw("rounded" + radius, size, size, radius + 1,
                (x, y) => new Color(1, 1, 1, RoundedAlpha(x, y, size, size, radius)));
        }

        /// <summary>Nút bấm kiểu 3D: thân sáng, mép dưới tối (tô màu bằng Image.color).</summary>
        public static Sprite Button(int radius = 36)
        {
            int size = radius * 2 + 8;
            int lip = Mathf.Max(6, radius / 3);
            return GetOrDraw("button" + radius, size, size, radius + 4, (x, y) =>
            {
                float v = y < lip ? 0.6f : Mathf.Lerp(0.82f, 1f, Mathf.InverseLerp(lip, size * 0.75f, y));
                return new Color(v, v, v, RoundedAlpha(x, y, size, size, radius));
            });
        }

        /// <summary>Khối vuông bóng có viền vát giống ô trong Block Blast.</summary>
        public static Sprite Block(Color c)
        {
            const int S = 64;
            const float Radius = 9f;
            const int Bevel = 7; // độ dày viền vát

            Color top = Color.Lerp(c, Color.white, 0.45f);
            Color left = Color.Lerp(c, Color.white, 0.2f);
            Color bottom = Color.Lerp(c, Color.black, 0.38f);
            Color right = Color.Lerp(c, Color.black, 0.2f);
            Color faceLow = Color.Lerp(c, Color.black, 0.06f);
            Color faceHigh = Color.Lerp(c, Color.white, 0.12f);

            Color Pixel(int x, int y)
            {
                int toLeft = x, toRight = S - 1 - x, toBottom = y, toTop = S - 1 - y;
                int nearest = Mathf.Min(Mathf.Min(toLeft, toRight), Mathf.Min(toBottom, toTop));
                Color col;
                if (nearest < Bevel)
                {
                    // viền vát: mỗi cạnh một sắc độ để khối trông nổi khối
                    if (nearest == toTop) col = top;
                    else if (nearest == toLeft) col = left;
                    else if (nearest == toBottom) col = bottom;
                    else col = right;
                }
                else
                {
                    col = Color.Lerp(faceLow, faceHigh, (float)y / S);
                    // vệt sáng nhỏ ở góc trên trái
                    bool shine = x >= Bevel + 3 && x < Bevel + 10 && y <= S - Bevel - 4 && y > S - Bevel - 11;
                    if (shine) col = Color.Lerp(col, Color.white, 0.75f);
                }
                col.a = RoundedAlpha(x, y, S, S, Radius);
                return col;
            }

            return GetOrDraw("block" + ColorUtility.ToHtmlStringRGBA(c), S, S, 0, Pixel);
        }

        /// <summary>Dải màu dọc dùng làm nền.</summary>
        public static Sprite Gradient(Color top, Color bottom)
        {
            const int H = 256;
            string key = "gradient" + ColorUtility.ToHtmlStringRGB(top) + ColorUtility.ToHtmlStringRGB(bottom);
            return GetOrDraw(key, 4, H, 0, (x, y) => Color.Lerp(bottom, top, y / (H - 1f)));
        }

        /// <summary>Đốm sáng tròn mờ dần ra mép.</summary>
        public static Sprite Glow()
        {
            const int S = 64;
            const float R = S / 2f;
            return GetOrDraw("glow", S, S, 0, (x, y) =>
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(R, R)) / R;
                float a = Mathf.Clamp01(1f - distance);
                return new Color(1, 1, 1, a * a);
            });
        }

        /// <summary>Lấy sprite trong cache, nếu chưa có thì vẽ từng pixel bằng hàm pixel(x, y).</summary>
        static Sprite GetOrDraw(string key, int width, int height, float border, Func<int, int, Color> pixel)
        {
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = pixel(x, y);

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels(pixels);
            tex.Apply(false, true);

            var sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, Vector4.one * border);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Độ phủ (alpha) của hình chữ nhật bo góc tại pixel (x, y), có khử răng cưa.</summary>
        static float RoundedAlpha(int x, int y, int width, int height, float radius)
        {
            float px = x + 0.5f, py = y + 0.5f;
            float dx = px - Mathf.Clamp(px, radius, width - radius);
            float dy = py - Mathf.Clamp(py, radius, height - radius);
            return Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
        }
    }
}

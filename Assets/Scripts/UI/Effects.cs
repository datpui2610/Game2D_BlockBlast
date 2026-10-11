using UnityEngine;

namespace BlockBlast
{
    /// <summary>Lớp hiệu ứng hình ảnh (VFX): khối vỡ, hạt văng, chữ điểm bay lên, banner.</summary>
    public class Effects : MonoBehaviour
    {
        const float Gravity = 1200f;

        Sprite[] blockSprites;

        public static Effects Create(Transform parent, Vector2 size, Sprite[] blockSprites)
        {
            var effects = UIKit.MakeRect("Effects", parent, size, Vector2.zero).gameObject.AddComponent<Effects>();
            effects.blockSprites = blockSprites;
            return effects;
        }

        public void Clear() => UIKit.DestroyChildren(transform);

        /// <summary>Khối tại vị trí world loé sáng, thu nhỏ, mờ dần rồi văng hạt ra xung quanh.</summary>
        public void BreakBlock(Vector3 world, int color, float delay)
        {
            var size = Vector2.one * (BoardView.CellSize - 4);
            var img = UIKit.MakeImage("Break", transform, blockSprites[color], Color.white, size, Vector2.zero);
            img.transform.position = world;
            var flash = UIKit.MakeImage("Flash", img.transform, SpriteFactory.RoundedRect(12), Color.clear, size,
                Vector2.zero, true);

            var rt = img.rectTransform;
            Tweener.Run(img, 0.32f, t =>
            {
                rt.localScale = Vector3.one * Mathf.Lerp(1.12f, 0f, Ease.InCubic(t));
                img.color = new Color(1, 1, 1, 1f - t * t);
                flash.color = new Color(1, 1, 1, 0.85f * (1f - t));
            }, () => Destroy(img.gameObject), delay);

            Tweener.Delay(img, delay, () => SpawnParticles(world, color, 4));
        }

        void SpawnParticles(Vector3 world, int color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float size = Random.Range(22f, 40f);
                var img = UIKit.MakeImage("Particle", transform, blockSprites[color], Color.white,
                    new Vector2(size, size), Vector2.zero);
                img.transform.position = world;

                var rt = img.rectTransform;
                Vector2 start = rt.anchoredPosition;
                var velocity = new Vector2(Random.Range(-450f, 450f), Random.Range(250f, 850f));
                float spin = Random.Range(-720f, 720f);
                float life = Random.Range(0.5f, 0.8f);
                Tweener.Run(img, life, t =>
                {
                    float time = t * life;
                    rt.anchoredPosition = start + velocity * time + new Vector2(0, -Gravity * time * time);
                    rt.localRotation = Quaternion.Euler(0, 0, spin * time);
                    img.color = new Color(1, 1, 1, 1f - t * t);
                }, () => Destroy(img.gameObject));
            }
        }

        /// <summary>Chữ bật ra rồi bay lên và mờ dần (vd: "+120").</summary>
        public void FloatingText(string text, Vector3 world, int fontSize, Color color)
        {
            var label = UIKit.MakeText("FloatingText", transform, text, fontSize, color, new Vector2(500, 120),
                Vector2.zero, TextAnchor.MiddleCenter, 4f);
            label.transform.position = world;

            var rt = label.rectTransform;
            Vector2 start = rt.anchoredPosition;
            Tweener.Run(label, 0.9f, t =>
            {
                rt.anchoredPosition = start + new Vector2(0, 140f * Ease.OutCubic(t));
                rt.localScale = Vector3.one * Mathf.Min(1f, Ease.OutBack(Mathf.Min(1f, t * 4f)));
                label.color = WithAlpha(color, t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f);
            }, () => Destroy(label.gameObject));
        }

        /// <summary>Dòng chữ lớn bật ra tại position rồi bay lên, mờ dần (GREAT!, COMBO x2...).</summary>
        public void Banner(string text, Color color, Vector2 position, int fontSize, float delay = 0f)
        {
            var label = UIKit.MakeText("Banner", transform, text, fontSize, color, new Vector2(1000, 260), position,
                TextAnchor.MiddleCenter, 6f);
            var rt = label.rectTransform;
            rt.localScale = Vector3.zero;

            Tweener.Run(label, 1.3f, t =>
            {
                rt.localScale = Vector3.one * Ease.OutBack(Mathf.Min(1f, t / 0.25f));
                float fade = t < 0.7f ? 0f : (t - 0.7f) / 0.3f;
                rt.anchoredPosition = position + new Vector2(0, 80f * fade);
                label.color = WithAlpha(color, 1f - fade);
            }, () => Destroy(label.gameObject), delay);
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}

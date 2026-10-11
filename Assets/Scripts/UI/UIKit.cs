using System;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>Các hàm tiện ích tạo UI bằng code.</summary>
    public static class UIKit
    {
        static Font font;

        /// <summary>Font trong Assets/Resources/BlockBlast/Font; không có thì dùng font mặc định của Unity.</summary>
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.Load<Font>("BlockBlast/Font");
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        /// <summary>Canvas phủ màn hình, tự co giãn theo độ phân giải tham chiếu.</summary>
        public static Canvas MakeCanvas(string name, Transform parent, Vector2 referenceResolution)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return canvas;
        }

        public static RectTransform MakeRect(string name, Transform parent, Vector2 size, Vector2 pos)
        {
            var rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        /// <summary>Kéo giãn RectTransform phủ kín đối tượng cha.</summary>
        public static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Image MakeImage(string name, Transform parent, Sprite sprite, Color color, Vector2 size,
            Vector2 pos, bool sliced = false)
        {
            var img = MakeRect(name, parent, size, pos).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            if (sliced) img.type = Image.Type.Sliced;
            return img;
        }

        public static Text MakeText(string name, Transform parent, string text, int fontSize, Color color,
            Vector2 size, Vector2 pos, TextAnchor anchor = TextAnchor.MiddleCenter, float outline = 3f)
        {
            var label = MakeRect(name, parent, size, pos).gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            if (outline > 0f)
            {
                var effect = label.gameObject.AddComponent<Outline>();
                effect.effectColor = Theme.TextOutline;
                effect.effectDistance = new Vector2(outline, -outline);
            }
            return label;
        }

        /// <summary>Nút bấm kiểu 3D, có âm click và hiệu ứng nhún. label rỗng = nút không có chữ.</summary>
        public static Button MakeButton(string name, Transform parent, Color color, Vector2 size, Vector2 pos,
            string label, int fontSize, Action onClick)
        {
            var img = MakeImage(name, parent, SpriteFactory.Button(), color, size, pos, true);
            img.raycastTarget = true;
            img.gameObject.AddComponent<ButtonPress>();

            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                SoundManager.Instance.Play(Sfx.Click);
                onClick?.Invoke();
            });

            if (!string.IsNullOrEmpty(label))
                MakeText("Label", img.transform, label, fontSize, Color.white, size, new Vector2(0, 5));
            return button;
        }

        public static void DestroyChildren(Transform parent)
        {
            foreach (Transform child in parent) UnityEngine.Object.Destroy(child.gameObject);
        }
    }
}

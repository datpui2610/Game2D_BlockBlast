using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>Một khối trong khay bên dưới: tự dựng hình, chạy animation và báo sự kiện kéo thả.</summary>
    public class PieceView : MonoBehaviour, IInitializePotentialDragHandler, IPointerDownHandler, IDragHandler,
        IPointerUpHandler
    {
        const float TrayScale = 0.52f; // khối được thu nhỏ khi nằm trong khay

        public event Action<PieceView, PointerEventData> Pressed, Dragged, Released;

        public Shape Shape { get; private set; }
        public int ColorIndex { get; private set; }
        public RectTransform Rect { get; private set; }

        Transform slot; // ô trong khay, nơi khối quay về khi thả sai chỗ
        CanvasGroup group;
        Coroutine anim; // animation đang chạy, bị huỷ khi bắt đầu animation mới

        public static PieceView Create(Transform slot, Shape shape, int colorIndex, Sprite sprite)
        {
            var rect = UIKit.MakeRect("Piece", slot, new Vector2(640, 640), Vector2.zero);
            rect.localScale = Vector3.zero;
            rect.gameObject.AddComponent<Image>().color = Color.clear; // vùng chạm trong suốt, giúp dễ cầm khối

            const float cell = BoardView.CellSize;
            foreach (var c in shape.Cells)
            {
                Vector2 offset = c - shape.Center;
                UIKit.MakeImage("Block", rect, sprite, Color.white, new Vector2(cell - 4, cell - 4),
                    new Vector2(offset.x, -offset.y) * cell);
            }

            var piece = rect.gameObject.AddComponent<PieceView>();
            piece.Shape = shape;
            piece.ColorIndex = colorIndex;
            piece.Rect = rect;
            piece.slot = slot;
            piece.group = rect.gameObject.AddComponent<CanvasGroup>();
            return piece;
        }

        /// <summary>Hiện ra trong khay với hiệu ứng bật nảy.</summary>
        public void PopIn(float delay) => Animate(0.35f, t => SetScale(TrayScale * Ease.OutBack(t)), delay);

        /// <summary>Nhấc khối sang lớp kéo thả và phóng về kích thước thật.</summary>
        public void PickUp(Transform dragLayer)
        {
            Rect.SetParent(dragLayer, true);
            float from = Rect.localScale.x;
            Animate(0.1f, t => SetScale(Mathf.LerpUnclamped(from, 1f, Ease.OutCubic(t))));
        }

        /// <summary>Bay về ô của mình trong khay.</summary>
        public void ReturnToSlot()
        {
            Rect.SetParent(slot, true);
            Vector2 fromPos = Rect.anchoredPosition;
            float fromScale = Rect.localScale.x;
            Animate(0.2f, t =>
            {
                float k = Ease.OutCubic(t);
                Rect.anchoredPosition = Vector2.LerpUnclamped(fromPos, Vector2.zero, k);
                SetScale(Mathf.LerpUnclamped(fromScale, TrayScale, k));
            });
        }

        /// <summary>Khối không còn chỗ đặt thì bị làm mờ.</summary>
        public void SetPlaceable(bool placeable) => group.alpha = placeable ? 1f : 0.35f;

        void Animate(float duration, Action<float> step, float delay = 0f)
        {
            Tweener.Stop(anim);
            anim = Tweener.Run(this, duration, step, null, delay);
        }

        void SetScale(float scale) => Rect.localScale = Vector3.one * scale;

        public void OnInitializePotentialDrag(PointerEventData e) => e.useDragThreshold = false;
        public void OnPointerDown(PointerEventData e) => Pressed?.Invoke(this, e);
        public void OnDrag(PointerEventData e) => Dragged?.Invoke(this, e);
        public void OnPointerUp(PointerEventData e) => Released?.Invoke(this, e);
    }
}

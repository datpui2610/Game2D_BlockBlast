using UnityEngine;
using UnityEngine.EventSystems;

namespace BlockBlast
{
    /// <summary>Hiệu ứng nhún khi nhấn nút.</summary>
    public class ButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData e) => transform.localScale = Vector3.one * 0.93f;
        public void OnPointerUp(PointerEventData e) => transform.localScale = Vector3.one;
        void OnDisable() => transform.localScale = Vector3.one;
    }
}

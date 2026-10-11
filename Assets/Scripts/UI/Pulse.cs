using UnityEngine;

namespace BlockBlast
{
    /// <summary>Phóng to / thu nhỏ nhịp nhàng liên tục (nút PLAY, chữ NEW BEST!).</summary>
    public class Pulse : MonoBehaviour
    {
        [SerializeField] float amount = 0.05f; // biên độ, 0.05 = to / nhỏ 5%
        [SerializeField] float speed = 4f;

        public static void Attach(Component target, float amount, float speed)
        {
            var pulse = target.gameObject.AddComponent<Pulse>();
            pulse.amount = amount;
            pulse.speed = speed;
        }

        void Update() => transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * speed) * amount);
    }
}

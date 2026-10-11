using System;
using System.Collections;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Hệ thống tween đơn giản bằng coroutine, chạy theo thời gian thực (không phụ thuộc Time.timeScale).
    /// Tween tự dừng khi đối tượng "owner" bị Destroy.
    /// </summary>
    public class Tweener : MonoBehaviour
    {
        static Tweener instance;

        static Tweener Instance
        {
            get
            {
                if (instance == null) instance = new GameObject(nameof(Tweener)).AddComponent<Tweener>();
                return instance;
            }
        }

        /// <summary>
        /// Sau delay giây, gọi step(t) mỗi frame với t chạy từ 0 đến 1 trong duration giây, xong thì gọi done.
        /// </summary>
        public static Coroutine Run(UnityEngine.Object owner, float duration, Action<float> step,
            Action done = null, float delay = 0f)
        {
            return Instance.StartCoroutine(Routine(owner, duration, step, done, delay));
        }

        /// <summary>Gọi action sau delay giây.</summary>
        public static Coroutine Delay(UnityEngine.Object owner, float delay, Action action) =>
            Run(owner, 0f, null, action, delay);

        /// <summary>Dừng một tween đang chạy (bỏ qua nếu null hoặc đã xong).</summary>
        public static void Stop(Coroutine tween)
        {
            if (tween != null && instance != null) instance.StopCoroutine(tween);
        }

        /// <summary>Phóng to tới scale rồi thu về kích thước cũ (hiệu ứng "nảy").</summary>
        public static Coroutine Punch(Transform target, float scale, float duration) =>
            Run(target, duration,
                t => target.localScale = Vector3.one * Mathf.LerpUnclamped(1f, scale, Mathf.Sin(t * Mathf.PI)));

        static IEnumerator Routine(UnityEngine.Object owner, float duration, Action<float> step, Action done,
            float delay)
        {
            for (float d = 0f; d < delay; d += Time.unscaledDeltaTime)
            {
                yield return null;
                if (owner == null) yield break;
            }

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (owner == null) yield break;
                step?.Invoke(t / duration);
                yield return null;
            }

            if (owner == null) yield break;
            step?.Invoke(1f);
            done?.Invoke();
        }
    }

    /// <summary>Các hàm làm mượt chuyển động: nhận t trong khoảng 0..1, trả về tiến độ đã làm mượt.</summary>
    public static class Ease
    {
        public static float InCubic(float t) => t * t * t;

        public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        /// <summary>Vượt quá đích một chút rồi quay về (hiệu ứng bật nảy).</summary>
        public static float OutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}

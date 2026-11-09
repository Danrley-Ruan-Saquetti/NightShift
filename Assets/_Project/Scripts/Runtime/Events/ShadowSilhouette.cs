using System.Collections;
using UnityEngine;

namespace NightShift
{
    public class ShadowSilhouette : MonoBehaviour
    {
        public bool IsCrossing { get; private set; }

        private Coroutine crossing;

        public void Cross(Vector3 from, Vector3 to, float duration)
        {
            Hide();
            gameObject.SetActive(true);
            IsCrossing = true;
            crossing = StartCoroutine(Slide(from, to, duration));
        }

        public void Hide()
        {
            if (crossing != null)
            {
                StopCoroutine(crossing);
                crossing = null;
            }

            IsCrossing = false;
            gameObject.SetActive(false);
        }

        private IEnumerator Slide(Vector3 from, Vector3 to, float duration)
        {
            Vector3 direction = to - from;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));

                yield return null;
            }

            crossing = null;
            Hide();
        }
    }
}

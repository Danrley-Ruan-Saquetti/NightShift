using System.Collections;
using UnityEngine;

namespace NightShift
{
    public class JumpscareFigure : MonoBehaviour
    {
        [SerializeField]
        private AudioSource screamSource;
        [SerializeField]
        private AudioClip screamClip;
        [SerializeField]
        private float jitterAmplitude = 0.004f;
        [SerializeField]
        private float jitterFrequency = 30f;

        public bool IsShowing { get; private set; }

        private Coroutine routine;

        public void Show(Vector3 headPosition, Vector3 direction, float startDistance, float endDistance, float advanceDuration)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            IsShowing = true;
            gameObject.SetActive(true);
            Pose start = JumpscarePlacement.Place(headPosition, direction, startDistance);
            transform.SetPositionAndRotation(start.position, start.rotation);

            screamSource.ignoreListenerPause = true;
            screamSource.clip = screamClip;
            screamSource.Play();

            routine = StartCoroutine(Animate(headPosition, direction, startDistance, endDistance, advanceDuration));
        }

        private IEnumerator Animate(Vector3 headPosition, Vector3 direction, float startDistance, float endDistance, float advanceDuration)
        {
            float elapsed = 0f;
            Quaternion rotation = transform.rotation;

            while (elapsed < advanceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / advanceDuration);
                float eased = 1f - (1f - progress) * (1f - progress);
                float distance = Mathf.Lerp(startDistance, endDistance, eased);
                transform.position = JumpscarePlacement.Place(headPosition, direction, distance).position;
                yield return null;
            }

            Vector3 restPosition = JumpscarePlacement.Place(headPosition, direction, endDistance).position;
            float time = 0f;

            while (true)
            {
                time += Time.unscaledDeltaTime * jitterFrequency;
                float x = Mathf.PerlinNoise(time, 0f) - 0.5f;
                float y = Mathf.PerlinNoise(0f, time) - 0.5f;
                Vector3 side = Vector3.Cross(Vector3.up, direction);
                transform.position = restPosition + (side * x + Vector3.up * y) * (2f * jitterAmplitude);
                transform.rotation = rotation;
                yield return null;
            }
        }
    }
}

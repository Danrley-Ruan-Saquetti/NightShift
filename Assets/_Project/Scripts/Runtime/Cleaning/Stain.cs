using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NightShift
{
    [RequireComponent(typeof(Collider))]
    public class Stain : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private TaskRegistry registry;
        [SerializeField]
        private string taskId;
        [SerializeField]
        private bool requiresSpray;
        [SerializeField]
        private float requiredScrubTime = 3f;
        [SerializeField]
        private float fadeDuration = 1f;
        [SerializeField, Range(0f, 1f)]
        private float minScrubAlpha = 0.4f;
        [SerializeField]
        private Renderer stainRenderer;
        [SerializeField]
        private Color sprayedColor = new(0.22f, 0.24f, 0.28f);

        private readonly HashSet<Sponge> touchingSponges = new();
        private StainCleaning cleaning;
        private Collider stainCollider;
        private MaterialPropertyBlock propertyBlock;
        private Color color;

        private void Awake()
        {
            cleaning = new StainCleaning(requiredScrubTime, requiresSpray);
            stainCollider = GetComponent<Collider>();
            propertyBlock = new MaterialPropertyBlock();
            color = stainRenderer.sharedMaterial.GetColor(BaseColorId);
        }

        private void OnEnable()
        {
            cleaning.Cleaned += OnCleaned;
        }

        private void Update()
        {
            if (cleaning.IsCleaned || !IsBeingScrubbed())
            {
                return;
            }

            cleaning.Scrub(Time.deltaTime);

            if (!cleaning.IsCleaned)
            {
                ApplyAlpha(Mathf.Lerp(1f, minScrubAlpha, cleaning.Progress));
            }
        }

        private void OnDisable()
        {
            cleaning.Cleaned -= OnCleaned;
        }

        public void Spray()
        {
            if (cleaning.IsSprayed)
            {
                return;
            }

            cleaning.Spray();
            color = new Color(sprayedColor.r, sprayedColor.g, sprayedColor.b, color.a);
            ApplyColor();
        }

        private bool IsBeingScrubbed()
        {
            foreach (Sponge sponge in touchingSponges)
            {
                if (sponge.IsHeld())
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyAlpha(float alpha)
        {
            color.a = alpha;
            ApplyColor();
        }

        private void ApplyColor()
        {
            propertyBlock.SetColor(BaseColorId, color);
            stainRenderer.SetPropertyBlock(propertyBlock);
        }

        private IEnumerator FadeOut()
        {
            float startAlpha = color.a;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                ApplyAlpha(Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration));
                yield return null;
            }

            gameObject.SetActive(false);
        }

        private void OnCleaned()
        {
            registry.Progress.Complete(taskId);
            stainCollider.enabled = false;
            touchingSponges.Clear();
            StartCoroutine(FadeOut());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out Sponge sponge))
            {
                touchingSponges.Add(sponge);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out Sponge sponge))
            {
                touchingSponges.Remove(sponge);
            }
        }
    }
}

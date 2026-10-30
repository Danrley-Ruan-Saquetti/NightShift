using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NightShift
{
    public class SceneTransition : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField]
        private float fadeOutDuration = 0.5f;
        [SerializeField]
        private float fadeInDuration = 0.5f;
        [SerializeField]
        private Renderer overlay;

        public static SceneTransition Instance { get; private set; }

        private readonly List<XRBaseInteractor> blockedInteractors = new List<XRBaseInteractor>();
        private Material overlayMaterial;
        private float alpha = 1f;
        private bool isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            overlayMaterial = overlay.material;
            ApplyAlpha(1f);
        }

        private void Start()
        {
            if (Instance == this)
            {
                StartCoroutine(EnterScene());
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadScene(string sceneName)
        {
            if (isTransitioning)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Scene '{sceneName}' is not in the build settings", this);
                return;
            }

            StartCoroutine(ExitScene(sceneName));
        }

        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        private IEnumerator ExitScene(string sceneName)
        {
            isTransitioning = true;
            BlockInteractors();
            yield return FadeTo(1f, fadeOutDuration);
            overlay.transform.SetParent(transform, false);
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return EnterScene();
        }

        private IEnumerator EnterScene()
        {
            isTransitioning = true;
            AttachOverlayToCamera();
            BlockInteractors();
            yield return FadeTo(0f, fadeInDuration);
            UnblockInteractors();
            isTransitioning = false;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (duration <= 0f)
            {
                ApplyAlpha(targetAlpha);
                yield break;
            }

            while (!Mathf.Approximately(alpha, targetAlpha))
            {
                ApplyAlpha(Mathf.MoveTowards(alpha, targetAlpha, Time.unscaledDeltaTime / duration));
                yield return null;
            }
        }

        private void ApplyAlpha(float value)
        {
            alpha = value;
            overlayMaterial.SetColor(ColorId, new Color(0f, 0f, 0f, alpha));
            AudioListener.volume = 1f - alpha;
            overlay.enabled = alpha > 0f;
        }

        private void AttachOverlayToCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera is missing, fade overlay cannot follow the player", this);
                return;
            }

            overlay.transform.SetParent(mainCamera.transform, false);
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localRotation = Quaternion.identity;
        }

        private void BlockInteractors()
        {
            foreach (XRBaseInteractor interactor in FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None))
            {
                if (interactor.enabled)
                {
                    interactor.enabled = false;
                    blockedInteractors.Add(interactor);
                }
            }
        }

        private void UnblockInteractors()
        {
            foreach (XRBaseInteractor interactor in blockedInteractors)
            {
                if (interactor != null)
                {
                    interactor.enabled = true;
                }
            }

            blockedInteractors.Clear();
        }
    }
}

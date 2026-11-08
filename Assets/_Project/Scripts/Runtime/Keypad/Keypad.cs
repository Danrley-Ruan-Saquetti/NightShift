using System.Collections;
using TMPro;
using UnityEngine;

namespace NightShift
{
    public class Keypad : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private string code = "8362";
        [SerializeField]
        private TMP_Text display;
        [SerializeField]
        private Renderer displayBackground;
        [SerializeField]
        private Color idleColor = new(0.04f, 0.07f, 0.05f);
        [SerializeField]
        private Color successColor = new(0.1f, 0.6f, 0.2f);
        [SerializeField]
        private Color errorColor = new(0.7f, 0.05f, 0.05f);
        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private AudioClip keyClip;
        [SerializeField]
        private AudioClip errorClip;
        [SerializeField]
        private AudioClip unlockClip;
        [SerializeField]
        private HingedDoor door;
        [SerializeField]
        private TaskRegistry registry;
        [SerializeField]
        private string taskId = "P3";
        [SerializeField]
        private float doorOpenAngle = 60f;
        [SerializeField]
        private float doorOpenDuration = 1.5f;
        [SerializeField]
        private int blinkCount = 3;
        [SerializeField]
        private float blinkDuration = 1f;

        private KeypadEntry entry;
        private MaterialPropertyBlock propertyBlock;
        private bool isBlinking;

        private void Awake()
        {
            entry = new KeypadEntry(code);
            propertyBlock = new MaterialPropertyBlock();
            display.text = entry.Digits;
            ApplyDisplayColor(idleColor);
        }

        private void OnEnable()
        {
            entry.Unlocked += OnUnlocked;
            entry.Rejected += OnRejected;
        }

        private void OnDisable()
        {
            entry.Unlocked -= OnUnlocked;
            entry.Rejected -= OnRejected;
        }

        public void Press(KeypadKeyType type, int digit)
        {
            if (entry.IsUnlocked || isBlinking)
            {
                return;
            }

            audioSource.PlayOneShot(keyClip);

            switch (type)
            {
                case KeypadKeyType.Digit:
                    entry.PressDigit(digit);
                    break;
                case KeypadKeyType.Erase:
                    entry.Erase();
                    break;
                case KeypadKeyType.Confirm:
                    entry.Confirm();
                    break;
            }

            display.text = entry.Digits;
        }

        private void ApplyDisplayColor(Color color)
        {
            propertyBlock.SetColor(BaseColorId, color);
            displayBackground.SetPropertyBlock(propertyBlock);
        }

        private IEnumerator Blink()
        {
            isBlinking = true;
            float halfCycle = blinkDuration / (blinkCount * 2);

            for (int i = 0; i < blinkCount; i++)
            {
                ApplyDisplayColor(errorColor);
                yield return new WaitForSeconds(halfCycle);
                ApplyDisplayColor(idleColor);
                yield return new WaitForSeconds(halfCycle);
            }

            isBlinking = false;
        }

        private void OnUnlocked()
        {
            audioSource.PlayOneShot(unlockClip);
            ApplyDisplayColor(successColor);
            door.Unlock();
            door.OpenTo(doorOpenAngle, doorOpenDuration);
            registry.Progress.Complete(taskId);
        }

        private void OnRejected()
        {
            audioSource.PlayOneShot(errorClip);
            StartCoroutine(Blink());
        }
    }
}

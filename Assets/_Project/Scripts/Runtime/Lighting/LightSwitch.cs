using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace NightShift
{
    public class LightSwitch : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private XRSimpleInteractable key;
        [SerializeField]
        private RoomLights lights;
        [SerializeField]
        private Renderer led;
        [SerializeField]
        private Color onColor = new(0.1f, 1f, 0.2f);
        [SerializeField]
        private Color offColor = new(0.5f, 0.03f, 0.03f);
        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private AudioClip clickClip;

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            key.selectEntered.AddListener(OnKeySelected);
            lights.StateChanged += OnLightsStateChanged;
            ApplyLed(lights.IsOn);
        }

        private void OnDisable()
        {
            key.selectEntered.RemoveListener(OnKeySelected);
            lights.StateChanged -= OnLightsStateChanged;
        }

        private void ApplyLed(bool isOn)
        {
            propertyBlock.SetColor(BaseColorId, isOn ? onColor : offColor);
            led.SetPropertyBlock(propertyBlock);
        }

        private void OnKeySelected(SelectEnterEventArgs _)
        {
            lights.Toggle();
            audioSource.PlayOneShot(clickClip);
        }

        private void OnLightsStateChanged(Room _, bool isOn)
        {
            ApplyLed(isOn);
        }
    }
}

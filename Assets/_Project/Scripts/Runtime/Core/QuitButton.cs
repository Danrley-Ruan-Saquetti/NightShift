using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace NightShift
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class QuitButton : MonoBehaviour
    {
        private XRSimpleInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelected);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelected);
        }

        private void OnSelected(SelectEnterEventArgs _)
        {
            Application.Quit();
        }
    }
}

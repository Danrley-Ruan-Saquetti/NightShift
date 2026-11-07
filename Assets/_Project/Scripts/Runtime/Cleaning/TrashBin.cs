using System.Collections.Generic;
using UnityEngine;

namespace NightShift
{
    [RequireComponent(typeof(Collider))]
    public class TrashBin : MonoBehaviour
    {
        private readonly HashSet<Trash> containedTrash = new();
        private readonly List<Trash> trashToDiscard = new();

        private void Update()
        {
            trashToDiscard.Clear();

            foreach (Trash trash in containedTrash)
            {
                if (!trash.IsHeld())
                {
                    trashToDiscard.Add(trash);
                }
            }

            foreach (Trash trash in trashToDiscard)
            {
                containedTrash.Remove(trash);
                trash.Discard();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out Trash trash))
            {
                containedTrash.Add(trash);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out Trash trash))
            {
                containedTrash.Remove(trash);
            }
        }
    }
}

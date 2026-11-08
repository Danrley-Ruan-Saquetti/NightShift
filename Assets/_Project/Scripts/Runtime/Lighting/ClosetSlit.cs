using UnityEngine;

namespace NightShift
{
    public class ClosetSlit : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private Renderer slit;
        [SerializeField]
        private RoomLights roomLights;
        [SerializeField]
        private Color litColor = new(1f, 0.85f, 0.55f);
        [SerializeField, Range(0f, 1f)]
        private float minimumBrightness = 0.03f;

        private MaterialPropertyBlock propertyBlock;
        private float appliedBrightness = -1f;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            float brightness = Mathf.Max(roomLights.Brightness, minimumBrightness);

            if (Mathf.Approximately(brightness, appliedBrightness))
            {
                return;
            }

            appliedBrightness = brightness;
            propertyBlock.SetColor(BaseColorId, litColor * brightness);
            slit.SetPropertyBlock(propertyBlock);
        }
    }
}

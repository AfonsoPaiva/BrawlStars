using UnityEngine;

namespace Assets.Scripts.Healthbars.Presenters
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasBillboard : MonoBehaviour
    {
        [Tooltip("If null, Camera.main will be used.")]
        [SerializeField] private Transform targetCamera;

        [Tooltip("If true, the billboard will stay upright (no X rotation).")]
        [SerializeField] private bool keepUpright = true;

        [Tooltip("Optional: scale the healthbar based on camera distance (0 = disabled).")]
        [SerializeField] private float distanceScaleFactor = 0f;

        [Tooltip("Clamp scale between these values when distance scaling is enabled.")]
        [SerializeField] private Vector2 scaleClamp = new Vector2(0.5f, 1.5f);

        private Transform _cam;

        private void OnEnable()
        {
            if (targetCamera != null)
            {
                _cam = targetCamera;
            }
            else if (Camera.main != null)
            {
                _cam = Camera.main.transform;
            }
            else
            {
                _cam = null;
            }
        }

        private void LateUpdate()
        {
            if (_cam == null)
            {
                if (Camera.main != null) _cam = Camera.main.transform;
                if (_cam == null) return;
            }

            Vector3 toCam = transform.position - _cam.position;
            if (toCam.sqrMagnitude < Mathf.Epsilon) return;

            if (keepUpright)
            {
                // Create a forward vector that ignores vertical difference so the UI remains upright
                Vector3 flat = new Vector3(toCam.x, 0f, toCam.z);
                if (flat.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(flat);
            }
            else
            {
                // Full billboard (will pitch/roll to face camera)
                transform.rotation = Quaternion.LookRotation(toCam);
            }

            // Optional: scale by distance to keep size reasonably constant
            if (!Mathf.Approximately(distanceScaleFactor, 0f))
            {
                float dist = Mathf.Clamp(toCam.magnitude, 0.01f, 100f);
                float s = Mathf.Clamp(1f + dist * distanceScaleFactor, scaleClamp.x, scaleClamp.y);
                transform.localScale = new Vector3(s, s, s);
            }
        }
    }
}

using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Keeps the board fully visible in portrait without stretching regardless of aspect ratio.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class BoardCameraFitter : MonoBehaviour
    {
        [SerializeField] private Transform boardRoot;
        [SerializeField] private float boardWorldSize = 10.5f;
        [SerializeField] private float padding = 0.7f;
        [SerializeField] private Vector3 cameraOffset = new(0f, -0.35f, -10f);

        private Camera targetCamera;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            FitCamera();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && targetCamera != null)
            {
                FitCamera();
            }
        }

        private void FitCamera()
        {
            if (targetCamera == null || !targetCamera.orthographic)
            {
                return;
            }

            float aspect = Mathf.Max(0.1f, targetCamera.aspect);
            float halfBoard = (boardWorldSize * 0.5f) + padding;

            float requiredVertical = halfBoard;
            float requiredHorizontal = halfBoard / aspect;
            targetCamera.orthographicSize = Mathf.Max(requiredVertical, requiredHorizontal);

            Vector3 focus = boardRoot != null ? boardRoot.position : Vector3.zero;
            targetCamera.transform.position = new Vector3(
                focus.x + cameraOffset.x,
                focus.y + cameraOffset.y,
                cameraOffset.z);
        }
    }
}

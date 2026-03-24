using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Applies portrait-friendly canvas scaling and optional safe-area fit.
    /// </summary>
    public class MobilePortraitSetup : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private RectTransform safeAreaPanel;

        private void Awake()
        {
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1080f, 1920f);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 1f;
            }

            ApplySafeArea();
        }
        public void Configure(CanvasScaler scalerToUse, RectTransform safeAreaTarget = null)
        {
            canvasScaler = scalerToUse;
            if (safeAreaTarget != null)
            {
                safeAreaPanel = safeAreaTarget;
            }

            Awake();
        }

        [ContextMenu("Apply Safe Area")]
        public void ApplySafeArea()
        {
            if (safeAreaPanel == null) return;

            Rect safeArea = Screen.safeArea;
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = safeArea.position + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            safeAreaPanel.anchorMin = minAnchor;
            safeAreaPanel.anchorMax = maxAnchor;
        }
    }
}

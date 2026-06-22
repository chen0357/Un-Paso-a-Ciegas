using UnityEngine;
using UnityEngine.UI;

public class VisionEffectCanvasRef : MonoBehaviour
{
    public GameObject blurryOverlay;
    public GameObject blindOverlay;

    private void Awake()
    {
        SetOverlayRaycastTarget(blurryOverlay, false);
        SetOverlayRaycastTarget(blindOverlay, false);
    }

    private void Start()
    {
        if (VisionModeManager.Instance != null)
            VisionModeManager.Instance.RegisterSceneOverlays(blurryOverlay, blindOverlay);
    }

    private static void SetOverlayRaycastTarget(GameObject overlay, bool raycastTarget)
    {
        if (overlay == null)
            return;

        var graphic = overlay.GetComponent<Graphic>();
        if (graphic != null)
            graphic.raycastTarget = raycastTarget;
    }
}

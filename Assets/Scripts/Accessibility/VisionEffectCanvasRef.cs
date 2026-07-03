using UnityEngine;
using UnityEngine.UI;

public class VisionEffectCanvasRef : MonoBehaviour
{
    public GameObject blurryOverlay;
    public GameObject nearlyBlindOverlay;
    public GameObject visualDisabilityOverlay;

    private void Awake()
    {
        SetOverlayRaycastTarget(blurryOverlay, false);
        SetOverlayRaycastTarget(nearlyBlindOverlay, false);
        SetOverlayRaycastTarget(visualDisabilityOverlay, false);

        if (blurryOverlay != null)
            blurryOverlay.SetActive(false);
        if (nearlyBlindOverlay != null)
            nearlyBlindOverlay.SetActive(false);
        if (visualDisabilityOverlay != null)
            visualDisabilityOverlay.SetActive(false);
    }

    private void Start()
    {
        if (VisionModeManager.Instance != null)
            VisionModeManager.Instance.RegisterSceneOverlays(blurryOverlay, nearlyBlindOverlay, visualDisabilityOverlay);
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

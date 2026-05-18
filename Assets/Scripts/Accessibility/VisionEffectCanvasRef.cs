using UnityEngine;

public class VisionEffectCanvasRef : MonoBehaviour
{
    public GameObject blurryOverlay;
    public GameObject blindOverlay;

    private void Start()
    {
        if (VisionModeManager.Instance != null)
        {
            VisionModeManager.Instance.RegisterSceneOverlays(blurryOverlay, blindOverlay);
        }
    }
}
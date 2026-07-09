using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// Lowers or raises the VR view height while keeping head tracking.
/// Camera Y Offset alone does not affect tracked headsets; this applies
/// a persistent offset to the camera floor offset after XR updates.
/// </summary>
[DefaultExecutionOrder(100)]
public class ViewHeightOffset : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;

    [Tooltip("Extra vertical offset in meters. Negative = lower view (e.g. -0.4).")]
    [SerializeField] private float heightOffset = -0.4f;

    private Transform cameraFloorOffset;

    private void Awake()
    {
        if (xrOrigin == null)
            xrOrigin = GetComponent<XROrigin>();

        CacheCameraFloorOffset();
    }

    private void LateUpdate()
    {
        if (cameraFloorOffset == null || xrOrigin == null)
            return;

        Vector3 localPosition = cameraFloorOffset.localPosition;
        float targetY = xrOrigin.CameraYOffset + heightOffset;

        if (Mathf.Approximately(localPosition.y, targetY))
            return;

        cameraFloorOffset.localPosition = new Vector3(localPosition.x, targetY, localPosition.z);
    }

    private void CacheCameraFloorOffset()
    {
        if (xrOrigin?.CameraFloorOffsetObject == null)
            return;

        cameraFloorOffset = xrOrigin.CameraFloorOffsetObject.transform;
    }
}

using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// Main menu only: disables XR movement providers and locks rig position.
/// Head look and snap/smooth turn remain available.
/// </summary>
public class MainMenuLocomotionRestrictor : MonoBehaviour
{
    private static readonly string[] MovementProviderNames =
    {
        "Move",
        "Teleportation",
        "Climb",
        "Grab Move"
    };

    [SerializeField] private XROrigin xrOrigin;

    private Vector3 fixedWorldPosition;
    private Transform cameraFloorOffset;

    private void Awake()
    {
        CacheReferences();
        DisableMovementProviders();
    }

    private void Start()
    {
        if (xrOrigin != null)
            fixedWorldPosition = xrOrigin.transform.position;
    }

    private void LateUpdate()
    {
        if (xrOrigin == null)
            return;

        Transform originTransform = xrOrigin.transform;
        if (originTransform.position != fixedWorldPosition)
            originTransform.position = fixedWorldPosition;

        if (cameraFloorOffset == null)
            return;

        Vector3 localPosition = cameraFloorOffset.localPosition;
        if (localPosition.x != 0f || localPosition.z != 0f)
            cameraFloorOffset.localPosition = new Vector3(0f, localPosition.y, 0f);
    }

    private void CacheReferences()
    {
        if (xrOrigin != null)
        {
            cameraFloorOffset = xrOrigin.CameraFloorOffsetObject != null
                ? xrOrigin.CameraFloorOffsetObject.transform
                : null;
            return;
        }

        xrOrigin = FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null)
            return;

        cameraFloorOffset = xrOrigin.CameraFloorOffsetObject != null
            ? xrOrigin.CameraFloorOffsetObject.transform
            : null;
    }

    private void DisableMovementProviders()
    {
        if (xrOrigin == null)
            return;

        Transform locomotion = xrOrigin.transform.Find("Locomotion");
        if (locomotion == null)
            return;

        for (int i = 0; i < MovementProviderNames.Length; i++)
        {
            Transform provider = locomotion.Find(MovementProviderNames[i]);
            if (provider != null)
                provider.gameObject.SetActive(false);
        }
    }
}

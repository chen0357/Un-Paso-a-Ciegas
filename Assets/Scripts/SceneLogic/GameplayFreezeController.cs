using UnityEngine;

/// <summary>
/// Disables XR locomotion while paused or after game over.
/// </summary>
public class GameplayFreezeController : MonoBehaviour
{
    [SerializeField] private GameObject locomotionRoot;

    private void Awake()
    {
        CacheLocomotionRoot();
    }

    private void LateUpdate()
    {
        if (locomotionRoot == null)
        {
            CacheLocomotionRoot();
            if (locomotionRoot == null)
                return;
        }

        bool frozen = GameManager.Instance != null && GameManager.Instance.IsGameplayFrozen();
        bool enableLocomotion = !frozen;

        if (locomotionRoot.activeSelf != enableLocomotion)
            locomotionRoot.SetActive(enableLocomotion);
    }

    private void CacheLocomotionRoot()
    {
        if (locomotionRoot != null)
            return;

        var xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin == null)
            return;

        Transform locomotion = xrOrigin.transform.Find("Locomotion");
        if (locomotion != null)
            locomotionRoot = locomotion.gameObject;
    }
}

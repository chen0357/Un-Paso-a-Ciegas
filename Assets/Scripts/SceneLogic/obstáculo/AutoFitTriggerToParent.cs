using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AutoFitTriggerToParent : MonoBehaviour
{
    [Header("Trigger Size Settings")]
    public float extraSize = 0.15f;

    private void Reset()
    {
        AutoFit();
    }

    private void OnValidate()
    {
        AutoFit();
    }

    [ContextMenu("Auto Fit Trigger")]
    public void AutoFit()
    {
        if (transform.parent == null) return;

        BoxCollider parentCollider = transform.parent.GetComponent<BoxCollider>();
        BoxCollider triggerCollider = GetComponent<BoxCollider>();

        if (parentCollider == null || triggerCollider == null) return;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        triggerCollider.isTrigger = true;
        triggerCollider.center = parentCollider.center;

        triggerCollider.size = new Vector3(
            parentCollider.size.x + extraSize,
            parentCollider.size.y + extraSize,
            parentCollider.size.z + extraSize
        );
    }
}
using UnityEngine;

/// <summary>
/// Replaces thin plane MeshColliders with a thick BoxCollider so the cane tip
/// trigger stays in contact and slide audio can play reliably.
/// </summary>
[DisallowMultipleComponent]
public class ThickGroundCollider : MonoBehaviour
{
    [SerializeField] private float thickness = 0.2f;
    [SerializeField] private bool disableMeshCollider = true;
    [SerializeField] private bool fitOnValidate = true;

    public float Thickness => thickness;

    private void Reset()
    {
        Apply();
    }

    private void OnValidate()
    {
        if (!fitOnValidate || !isActiveAndEnabled)
            return;

        Apply();
    }

    [ContextMenu("Apply Thick Collider")]
    public void Apply()
    {
        thickness = Mathf.Max(0.05f, thickness);

        if (!TryGetBounds(out Bounds localBounds))
            return;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        float surfaceY = localBounds.max.y;
        boxCollider.isTrigger = false;
        boxCollider.center = new Vector3(
            localBounds.center.x,
            surfaceY - thickness * 0.5f,
            localBounds.center.z);
        boxCollider.size = new Vector3(
            Mathf.Max(localBounds.size.x, 0.1f),
            thickness,
            Mathf.Max(localBounds.size.z, 0.1f));

        if (!disableMeshCollider)
            return;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
            meshCollider.enabled = false;
    }

    private bool TryGetBounds(out Bounds localBounds)
    {
        if (TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
        {
            localBounds = meshFilter.sharedMesh.bounds;
            return true;
        }

        if (TryGetComponent(out Renderer renderer))
        {
            Bounds worldBounds = renderer.bounds;
            localBounds = new Bounds(
                transform.InverseTransformPoint(worldBounds.center),
                transform.InverseTransformVector(worldBounds.size));
            localBounds.size = new Vector3(
                Mathf.Abs(localBounds.size.x),
                Mathf.Abs(localBounds.size.y),
                Mathf.Abs(localBounds.size.z));
            return true;
        }

        localBounds = default;
        return false;
    }
}

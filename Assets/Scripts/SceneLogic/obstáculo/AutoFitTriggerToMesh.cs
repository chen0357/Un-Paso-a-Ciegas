using UnityEngine;

/// <summary>
/// Fits a child BoxCollider trigger to visible mesh geometry under the parent building.
/// Use on the "Trigger Collider" child together with DamageObject.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class AutoFitTriggerToMesh : MonoBehaviour
{
    [Header("Trigger Size Settings")]
    [Tooltip("Extra padding in world units (meters), applied equally on all sides.")]
    public float extraSize = 0.15f;

    [Header("Mesh Source")]
    [Tooltip("Include MeshFilter / SkinnedMeshRenderer geometry from all children of the parent.")]
    public bool includeChildMeshes = true;

    [Tooltip("Also include MeshCollider meshes (useful when colliders live on child objects).")]
    public bool includeMeshColliders = true;

    private void Reset()
    {
        AutoFit();
    }

    private void OnValidate()
    {
        AutoFit();
    }

    [ContextMenu("Auto Fit Trigger To Mesh")]
    public void AutoFit()
    {
        if (transform.parent == null)
            return;

        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
            return;

        Transform parent = transform.parent;
        if (!TryGetBoundsInParentLocal(parent, out Bounds localBounds))
            return;

        if (extraSize > 0f)
        {
            Vector3 padding = WorldExtraToParentLocal(extraSize, parent);
            localBounds.Expand(padding);
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        triggerCollider.isTrigger = true;
        triggerCollider.center = localBounds.center;
        triggerCollider.size = localBounds.size;
    }

    private bool TryGetBoundsInParentLocal(Transform parent, out Bounds localBounds)
    {
        bool found = false;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;

        if (includeChildMeshes)
        {
            MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                if (ShouldSkipTransform(meshFilter.transform))
                    continue;

                found |= EncapsulateMeshBounds(
                    meshFilter.sharedMesh,
                    meshFilter.transform,
                    parent,
                    ref min,
                    ref max,
                    found
                );
            }

            SkinnedMeshRenderer[] skinnedMeshes = parent.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < skinnedMeshes.Length; i++)
            {
                SkinnedMeshRenderer skinnedMesh = skinnedMeshes[i];
                if (skinnedMesh == null || skinnedMesh.sharedMesh == null)
                    continue;

                if (ShouldSkipTransform(skinnedMesh.transform))
                    continue;

                found |= EncapsulateMeshBounds(
                    skinnedMesh.sharedMesh,
                    skinnedMesh.transform,
                    parent,
                    ref min,
                    ref max,
                    found
                );
            }
        }

        if (includeMeshColliders)
        {
            MeshCollider[] meshColliders = parent.GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < meshColliders.Length; i++)
            {
                MeshCollider meshCollider = meshColliders[i];
                if (meshCollider == null || !meshCollider.enabled || meshCollider.sharedMesh == null)
                    continue;

                if (ShouldSkipTransform(meshCollider.transform))
                    continue;

                found |= EncapsulateMeshBounds(
                    meshCollider.sharedMesh,
                    meshCollider.transform,
                    parent,
                    ref min,
                    ref max,
                    found
                );
            }
        }

        if (!found)
        {
            localBounds = default;
            return false;
        }

        localBounds = new Bounds();
        localBounds.SetMinMax(min, max);
        return true;
    }

    private bool ShouldSkipTransform(Transform source)
    {
        return source == transform || source.IsChildOf(transform);
    }

    private static bool EncapsulateMeshBounds(
        Mesh mesh,
        Transform meshTransform,
        Transform parent,
        ref Vector3 min,
        ref Vector3 max,
        bool hasBounds)
    {
        Bounds meshBounds = mesh.bounds;
        Vector3 center = meshBounds.center;
        Vector3 extents = meshBounds.extents;

        for (int xi = -1; xi <= 1; xi += 2)
        {
            for (int yi = -1; yi <= 1; yi += 2)
            {
                for (int zi = -1; zi <= 1; zi += 2)
                {
                    Vector3 cornerInMeshSpace = center + Vector3.Scale(extents, new Vector3(xi, yi, zi));
                    Vector3 worldCorner = meshTransform.TransformPoint(cornerInMeshSpace);
                    Vector3 parentLocalCorner = parent.InverseTransformPoint(worldCorner);

                    if (!hasBounds)
                    {
                        min = parentLocalCorner;
                        max = parentLocalCorner;
                        hasBounds = true;
                    }
                    else
                    {
                        min = Vector3.Min(min, parentLocalCorner);
                        max = Vector3.Max(max, parentLocalCorner);
                    }
                }
            }
        }

        return hasBounds;
    }

    private static Vector3 WorldExtraToParentLocal(float worldPadding, Transform parent)
    {
        Vector3 localPadding = parent.InverseTransformVector(Vector3.one * worldPadding);
        return new Vector3(
            Mathf.Abs(localPadding.x),
            Mathf.Abs(localPadding.y),
            Mathf.Abs(localPadding.z)
        );
    }
}

using UnityEditor;
using UnityEngine;

public static class ThickGroundColliderEditor
{
    private const string MenuPath = "Tools/Cane/Apply Thick Ground Colliders In Scene";

    [MenuItem(MenuPath)]
    private static void ApplyToScene()
    {
        SurfaceTag[] surfaceTags = Object.FindObjectsByType<SurfaceTag>(FindObjectsSortMode.None);
        int updated = 0;

        foreach (SurfaceTag surfaceTag in surfaceTags)
        {
            if (!IsGroundLike(surfaceTag.surfaceType))
                continue;

            ThickGroundCollider thickCollider = surfaceTag.GetComponent<ThickGroundCollider>();
            if (thickCollider == null)
                thickCollider = Undo.AddComponent<ThickGroundCollider>(surfaceTag.gameObject);

            Undo.RecordObject(thickCollider, "Apply Thick Ground Collider");
            thickCollider.Apply();
            EditorUtility.SetDirty(thickCollider);
            updated++;
        }

        Debug.Log($"Applied thick ground colliders to {updated} object(s).");
    }

    [MenuItem(MenuPath, true)]
    private static bool ApplyToSceneValidate()
    {
        return !Application.isPlaying;
    }

    private static bool IsGroundLike(SurfaceType surfaceType)
    {
        return surfaceType == SurfaceType.Ground || surfaceType == SurfaceType.TactilePaving;
    }
}

[CustomEditor(typeof(ThickGroundCollider))]
public class ThickGroundColliderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ThickGroundCollider thickCollider = (ThickGroundCollider)target;
        if (GUILayout.Button("Apply Thick Collider"))
        {
            Undo.RecordObject(thickCollider, "Apply Thick Ground Collider");
            thickCollider.Apply();
            EditorUtility.SetDirty(thickCollider);
        }
    }
}

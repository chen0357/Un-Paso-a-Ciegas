using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DangerZoneSidewalkEditor
{
    private const string SidewalkMaterialPath = "Assets/UI/Materia/renxing.mat";

    [MenuItem("Tools/Setup/Add Sidewalk to DangerRoadZone (4)")]
    public static void AddSidewalkToDangerRoadZone4()
    {
        GameObject dangerZone = GameObject.Find("DangerRoadZone (4)");
        if (dangerZone == null)
        {
            Debug.LogError("DangerZoneSidewalkEditor: Could not find 'DangerRoadZone (4)' in the open scene.");
            return;
        }

        AddSidewalkToDangerZone(dangerZone);
    }

    [MenuItem("Tools/Setup/Add Sidewalk to Selected Danger Zone")]
    public static void AddSidewalkToSelectedDangerZone()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("DangerZoneSidewalkEditor: Select a DangerRoadZone object first.");
            return;
        }

        if (Selection.activeGameObject.GetComponent<DangerZone>() == null)
        {
            Debug.LogError("DangerZoneSidewalkEditor: Selected object must have a DangerZone component.");
            return;
        }

        AddSidewalkToDangerZone(Selection.activeGameObject);
    }

    private static void AddSidewalkToDangerZone(GameObject dangerZone)
    {
        Transform existing = dangerZone.transform.Find("Sidewalk");
        if (existing != null)
        {
            Debug.LogWarning("DangerZoneSidewalkEditor: Sidewalk already exists under " + dangerZone.name + ".");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        Material sidewalkMaterial = AssetDatabase.LoadAssetAtPath<Material>(SidewalkMaterialPath);
        if (sidewalkMaterial == null)
        {
            Debug.LogError("DangerZoneSidewalkEditor: Missing material at " + SidewalkMaterialPath);
            return;
        }

        GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sidewalk.name = "Sidewalk";
        Undo.RegisterCreatedObjectUndo(sidewalk, "Add Danger Zone Sidewalk");

        sidewalk.transform.SetParent(dangerZone.transform, false);
        sidewalk.transform.localRotation = Quaternion.identity;

        float widthRatio = 0.25f;
        sidewalk.transform.localScale = new Vector3(widthRatio, 2f, 1f);
        sidewalk.transform.localPosition = new Vector3(-0.5f + widthRatio * 0.5f, 1f, 0f);

        Object.DestroyImmediate(sidewalk.GetComponent<BoxCollider>());

        BoxCollider trigger = sidewalk.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        sidewalk.AddComponent<SidewalkSafeZone>();
        sidewalk.GetComponent<MeshRenderer>().sharedMaterial = sidewalkMaterial;

        EditorSceneManager.MarkSceneDirty(sidewalk.scene);
        Selection.activeGameObject = sidewalk;

        Debug.Log("Sidewalk added under " + dangerZone.name + ". Adjust local Position/Scale in Inspector if needed.");
    }
}

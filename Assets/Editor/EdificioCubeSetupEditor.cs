using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EdificioCubeSetupEditor
{
    [MenuItem("Tools/Setup/Apply Edificio Cube Damage")]
    public static void ApplyEdificioCubeDamage()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int updatedCount = 0;

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject go = allObjects[i];
            if (go == null || !go.name.Contains("Cube"))
                continue;

            if (!IsUnderEdificio(go.transform))
                continue;

            if (go.tag != "BuildingObstacle")
            {
                Undo.RecordObject(go, "Set BuildingObstacle Tag");
                go.tag = "BuildingObstacle";
                updatedCount++;
            }

            if (go.GetComponent<DamageObject>() == null)
            {
                Undo.AddComponent<DamageObject>(go);
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"Edificio Cube setup complete. Updated entries: {updatedCount}");
        }
        else
        {
            Debug.Log("Edificio Cube setup complete. No changes were needed.");
        }
    }

    private static bool IsUnderEdificio(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name.StartsWith("Edificio"))
                return true;

            current = current.parent;
        }

        return false;
    }
}

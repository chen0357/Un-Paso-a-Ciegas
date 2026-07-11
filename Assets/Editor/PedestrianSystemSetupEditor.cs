using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PedestrianSystemSetupEditor
{
    private const string PrefabFolder = "Assets/Prefab/Pedestrians";
    private const string DefaultHitClipPath = "Assets/UI/Audio/shouji.mp3";

    private static readonly Color[] CapsuleColors =
    {
        new Color(0.25f, 0.55f, 0.95f),
        new Color(0.35f, 0.85f, 0.45f),
        new Color(0.95f, 0.45f, 0.35f)
    };

    private static readonly string[] CapsuleNames =
    {
        "Pedestrian_Capsule_Blue",
        "Pedestrian_Capsule_Green",
        "Pedestrian_Capsule_Red"
    };

    [MenuItem("Tools/Setup/Create Pedestrian Placeholder Prefabs")]
    public static void CreatePedestrianPrefabs()
    {
        EnsureFolder(PrefabFolder);

        var created = new GameObject[CapsuleNames.Length];
        for (int i = 0; i < CapsuleNames.Length; i++)
            created[i] = BuildPedestrianPrefabObject(CapsuleNames[i], CapsuleColors[i]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        for (int i = 0; i < created.Length; i++)
            Object.DestroyImmediate(created[i]);

        Debug.Log("PedestrianSystemSetupEditor: Created " + CapsuleNames.Length +
                  " placeholder prefabs in " + PrefabFolder + ".");
    }

    [MenuItem("Tools/Setup/Setup Full Pedestrian System (Level_Street)")]
    public static void SetupFullPedestrianSystem()
    {
        EditorSceneManager.OpenScene("Assets/Localizacion/Level_Street.unity");
        CreatePedestrianPrefabs();
        AddPedestrianSystemToScene();
    }

    [MenuItem("Tools/Setup/Add Pedestrian System to Scene")]
    public static void AddPedestrianSystemToScene()
    {
        if (GameObject.Find("PedestrianSystem") != null)
        {
            Debug.LogWarning("PedestrianSystemSetupEditor: PedestrianSystem already exists in the scene.");
            return;
        }

        GameObject[] prefabs = LoadPedestrianPrefabs();
        if (prefabs.Length == 0)
        {
            Debug.LogError("PedestrianSystemSetupEditor: No pedestrian prefabs found. Run Create Pedestrian Placeholder Prefabs first.");
            return;
        }

        GameObject root = new GameObject("PedestrianSystem");
        Undo.RegisterCreatedObjectUndo(root, "Add Pedestrian System");

        Transform laneNs = CreateChild(root.transform, "Lane_NS");
        Transform laneEw = CreateChild(root.transform, "Lane_EW");

        Transform pedSpawnNsWestN = CreateSpawnPoint(laneNs, "PedSpawn_NS_West_N",
            new Vector3(-8f, 1f, -149.5f), Quaternion.identity);
        Transform pedDespawnNsWestN = CreateSpawnPoint(laneNs, "PedDespawn_NS_West_N",
            new Vector3(-8f, 1f, 144.2f), Quaternion.identity);
        Transform pedSpawnNsWestS = CreateSpawnPoint(laneNs, "PedSpawn_NS_West_S",
            new Vector3(-8f, 1f, 144.2f), Quaternion.Euler(0f, 180f, 0f));
        Transform pedDespawnNsWestS = CreateSpawnPoint(laneNs, "PedDespawn_NS_West_S",
            new Vector3(-8f, 1f, -149.5f), Quaternion.Euler(0f, 180f, 0f));

        Transform pedSpawnNsEastN = CreateSpawnPoint(laneNs, "PedSpawn_NS_East_N",
            new Vector3(8f, 1f, -149.5f), Quaternion.identity);
        Transform pedDespawnNsEastN = CreateSpawnPoint(laneNs, "PedDespawn_NS_East_N",
            new Vector3(8f, 1f, 144.2f), Quaternion.identity);
        Transform pedSpawnNsEastS = CreateSpawnPoint(laneNs, "PedSpawn_NS_East_S",
            new Vector3(8f, 1f, 144.2f), Quaternion.Euler(0f, 180f, 0f));
        Transform pedDespawnNsEastS = CreateSpawnPoint(laneNs, "PedDespawn_NS_East_S",
            new Vector3(8f, 1f, -149.5f), Quaternion.Euler(0f, 180f, 0f));

        Transform pedSpawnEwNorthE = CreateSpawnPoint(laneEw, "PedSpawn_EW_North_E",
            new Vector3(-142.7f, 1f, 8f), Quaternion.Euler(0f, 90f, 0f));
        Transform pedDespawnEwNorthE = CreateSpawnPoint(laneEw, "PedDespawn_EW_North_E",
            new Vector3(149.9f, 1f, 8f), Quaternion.Euler(0f, 90f, 0f));
        Transform pedSpawnEwNorthW = CreateSpawnPoint(laneEw, "PedSpawn_EW_North_W",
            new Vector3(149.9f, 1f, 8f), Quaternion.Euler(0f, 270f, 0f));
        Transform pedDespawnEwNorthW = CreateSpawnPoint(laneEw, "PedDespawn_EW_North_W",
            new Vector3(-142.7f, 1f, 8f), Quaternion.Euler(0f, 270f, 0f));

        Transform pedSpawnEwSouthW = CreateSpawnPoint(laneEw, "PedSpawn_EW_South_W",
            new Vector3(149.9f, 1f, -8f), Quaternion.Euler(0f, 270f, 0f));
        Transform pedDespawnEwSouthW = CreateSpawnPoint(laneEw, "PedDespawn_EW_South_W",
            new Vector3(-142.7f, 1f, -8f), Quaternion.Euler(0f, 270f, 0f));
        Transform pedSpawnEwSouthE = CreateSpawnPoint(laneEw, "PedSpawn_EW_South_E",
            new Vector3(-142.7f, 1f, -8f), Quaternion.Euler(0f, 90f, 0f));
        Transform pedDespawnEwSouthE = CreateSpawnPoint(laneEw, "PedDespawn_EW_South_E",
            new Vector3(149.9f, 1f, -8f), Quaternion.Euler(0f, 90f, 0f));

        CreateSpawner(root.transform, "Spawner_NS", prefabs, new[]
        {
            new LaneSetup("West Sidewalk Northbound", pedSpawnNsWestN, pedDespawnNsWestN),
            new LaneSetup("West Sidewalk Southbound", pedSpawnNsWestS, pedDespawnNsWestS),
            new LaneSetup("East Sidewalk Northbound", pedSpawnNsEastN, pedDespawnNsEastN),
            new LaneSetup("East Sidewalk Southbound", pedSpawnNsEastS, pedDespawnNsEastS)
        }, 7f, 1f, 3.2f, 5);

        CreateSpawner(root.transform, "Spawner_EW", prefabs, new[]
        {
            new LaneSetup("North Sidewalk Eastbound", pedSpawnEwNorthE, pedDespawnEwNorthE),
            new LaneSetup("North Sidewalk Westbound", pedSpawnEwNorthW, pedDespawnEwNorthW),
            new LaneSetup("South Sidewalk Westbound", pedSpawnEwSouthW, pedDespawnEwSouthW),
            new LaneSetup("South Sidewalk Eastbound", pedSpawnEwSouthE, pedDespawnEwSouthE)
        }, 6f, 1f, 3.2f, 5);

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        Debug.Log("PedestrianSystemSetupEditor: PedestrianSystem added with 8 bidirectional sidewalk lanes and " +
                  prefabs.Length + " prefab variants.");
    }

    private static GameObject BuildPedestrianPrefabObject(string prefabName, Color color)
    {
        string prefabPath = PrefabFolder + "/" + prefabName + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null)
            return existing;

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        root.name = prefabName;
        root.transform.localScale = new Vector3(0.55f, 1f, 0.55f);

        Object.DestroyImmediate(root.GetComponent<CapsuleCollider>());

        CapsuleCollider bodyCollider = root.AddComponent<CapsuleCollider>();
        bodyCollider.height = 2f;
        bodyCollider.radius = 0.28f;
        bodyCollider.center = Vector3.zero;

        Renderer renderer = root.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        root.AddComponent<PedestrianWalker>();

        GameObject trigger = new GameObject("Damage Trigger");
        trigger.transform.SetParent(root.transform, false);
        trigger.transform.localScale = Vector3.one;

        CapsuleCollider triggerCollider = trigger.AddComponent<CapsuleCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.height = 2.1f;
        triggerCollider.radius = 0.35f;
        triggerCollider.center = Vector3.zero;

        DamageObject damage = trigger.AddComponent<DamageObject>();
        damage.damageAmount = 5;
        damage.damageCooldown = 1f;
        damage.hitHintMessage = "Peatón";

        AudioClip hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultHitClipPath);
        if (hitClip != null)
        {
            SerializedObject serializedDamage = new SerializedObject(damage);
            serializedDamage.FindProperty("hitClip").objectReferenceValue = hitClip;
            serializedDamage.ApplyModifiedPropertiesWithoutUndo();
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateSpawner(
        Transform parent,
        string spawnerName,
        GameObject[] prefabs,
        LaneSetup[] laneSetups,
        float spawnInterval,
        float minSpeed,
        float maxSpeed,
        int maxActive)
    {
        GameObject spawnerObject = new GameObject(spawnerName);
        spawnerObject.transform.SetParent(parent, false);

        PedestrianSpawner spawner = spawnerObject.AddComponent<PedestrianSpawner>();
        spawner.spawnInterval = spawnInterval;
        spawner.initialDelay = 2f;
        spawner.minSpeed = minSpeed;
        spawner.maxSpeed = maxSpeed;
        spawner.maxActivePedestrians = maxActive;
        spawner.flowAxis = spawnerName.Contains("_NS") || spawnerName.Contains("NS")
            ? TrafficFlowAxis.NorthSouth
            : TrafficFlowAxis.EastWest;
        spawner.releaseDelay = 0.5f;
        spawner.lateralOffsetRange = 0.75f;
        spawner.pedestrianPrefabs = new System.Collections.Generic.List<GameObject>(prefabs);

        spawner.lanes = new System.Collections.Generic.List<TrafficLaneConfig>();
        for (int i = 0; i < laneSetups.Length; i++)
        {
            LaneSetup lane = laneSetups[i];
            spawner.lanes.Add(new TrafficLaneConfig
            {
                laneName = lane.Name,
                spawnPoint = lane.Spawn,
                despawnPoint = lane.Despawn
            });
        }
    }

    private static Transform CreateSpawnPoint(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject point = new GameObject(name);
        point.transform.SetParent(parent, false);
        point.transform.position = position;
        point.transform.rotation = rotation;
        return point.transform;
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static GameObject[] LoadPedestrianPrefabs()
    {
        var prefabs = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < CapsuleNames.Length; i++)
        {
            string path = PrefabFolder + "/" + CapsuleNames[i] + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                prefabs.Add(prefab);
        }

        return prefabs.ToArray();
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string folderName = Path.GetFileName(folderPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private readonly struct LaneSetup
    {
        public LaneSetup(string name, Transform spawn, Transform despawn)
        {
            Name = name;
            Spawn = spawn;
            Despawn = despawn;
        }

        public string Name { get; }
        public Transform Spawn { get; }
        public Transform Despawn { get; }
    }
}

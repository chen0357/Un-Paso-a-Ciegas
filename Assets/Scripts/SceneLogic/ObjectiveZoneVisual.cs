using UnityEngine;

[DisallowMultipleComponent]
public class ObjectiveZoneVisual : MonoBehaviour
{
    [Header("Beacon Shape")]
    public float pillarHeight = 12f;
    public float pillarRadius = 0.22f;
    public float groundRadius = 1.1f;

    [Header("Trigger Area")]
    public bool fitTriggerToBeacon = true;
    public float triggerRadiusPadding = 0.35f;
    public float triggerHeight = 2f;
    public float triggerBaseOffset = 0f;

    [Header("Color")]
    public Color pillarColor = new Color(0.25f, 1f, 0.35f, 0.75f);
    public Color groundColor = new Color(0.35f, 1f, 0.45f, 0.55f);

    [Header("Animation")]
    public bool pulse = true;
    public float pulseSpeed = 1.6f;
    public float pulseAmount = 0.18f;

    [Header("Light")]
    public bool addPointLight = true;
    public float lightRange = 4f;
    public float lightIntensity = 1.4f;

    [Header("Lifecycle")]
    public bool hideWhenTaskCompleted = true;

    private Transform visualRoot;
    private Renderer pillarRenderer;
    private Renderer groundRenderer;
    private Light pointLight;
    private Material pillarMaterial;
    private Material groundMaterial;
    private Color pillarBaseColor;
    private Color groundBaseColor;
    private bool hidden;
    private bool subscribedToTaskManager;

    private void Awake()
    {
        DisableLegacyCubeVisual();
        BuildVisual();
        ConfigureTriggerCollider();
    }

    private void OnEnable()
    {
        TrySubscribeToTaskManager();
        UpdateVisibility();
    }

    private void Start()
    {
        TrySubscribeToTaskManager();
        ConfigureTriggerCollider();
        UpdateVisibility();
    }

    private void OnDisable()
    {
        UnsubscribeFromTaskManager();
    }

    private void Update()
    {
        TrySubscribeToTaskManager();
        UpdateVisibility();

        if (!pulse || hidden || pillarMaterial == null)
            return;

        float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
        float alphaBoost = 1f + pulseAmount * wave;

        pillarMaterial.color = new Color(
            pillarBaseColor.r,
            pillarBaseColor.g,
            pillarBaseColor.b,
            pillarBaseColor.a * alphaBoost);

        if (groundMaterial != null)
        {
            groundMaterial.color = new Color(
                groundBaseColor.r,
                groundBaseColor.g,
                groundBaseColor.b,
                groundBaseColor.a * alphaBoost);
        }

        if (pointLight != null)
            pointLight.intensity = lightIntensity * (0.85f + 0.15f * wave);
    }

    public void Hide()
    {
        if (hidden)
            return;

        hidden = true;

        if (visualRoot != null)
            visualRoot.gameObject.SetActive(false);
    }

    private void HandleTaskCompleted(string completedTaskId)
    {
        if (!hideWhenTaskCompleted)
            return;

        ObjectiveTriggerZone trigger = GetComponent<ObjectiveTriggerZone>();
        if (trigger != null && trigger.taskId == completedTaskId)
            Hide();

        GoalTrigger goalTrigger = GetComponent<GoalTrigger>();
        if (goalTrigger != null && goalTrigger.completeTaskId == completedTaskId)
            Hide();
    }

    private void HandleTasksChanged()
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (visualRoot == null)
            return;

        if (hidden)
        {
            visualRoot.gameObject.SetActive(false);
            return;
        }

        string relatedTaskId = GetRelatedTaskId();
        if (TaskManager.Instance == null || string.IsNullOrEmpty(relatedTaskId))
        {
            visualRoot.gameObject.SetActive(true);
            return;
        }

        bool shouldShow = TaskManager.Instance.IsTaskCurrent(relatedTaskId);
        visualRoot.gameObject.SetActive(shouldShow);
    }

    private void DisableLegacyCubeVisual()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void BuildVisual()
    {
        if (visualRoot != null)
            return;

        Shader beaconShader = Shader.Find("Custom/ObjectiveBeacon");
        if (beaconShader == null)
            beaconShader = Shader.Find("Universal Render Pipeline/Unlit");

        pillarMaterial = new Material(beaconShader);
        groundMaterial = new Material(beaconShader);

        ConfigureTransparentMaterial(pillarMaterial);
        ConfigureTransparentMaterial(groundMaterial);

        pillarBaseColor = pillarColor;
        groundBaseColor = groundColor;
        pillarMaterial.color = pillarBaseColor;
        groundMaterial.color = groundBaseColor;

        visualRoot = new GameObject("BeaconVisual").transform;
        visualRoot.SetParent(transform, false);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;

        Vector3 parentScale = transform.localScale;
        visualRoot.localScale = new Vector3(
            parentScale.x != 0f ? 1f / parentScale.x : 1f,
            parentScale.y != 0f ? 1f / parentScale.y : 1f,
            parentScale.z != 0f ? 1f / parentScale.z : 1f);

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.SetParent(visualRoot, false);
        pillar.transform.localPosition = new Vector3(0f, pillarHeight * 0.5f, 0f);
        pillar.transform.localScale = new Vector3(pillarRadius * 2f, pillarHeight * 0.5f, pillarRadius * 2f);
        DestroyCollider(pillar);
        pillarRenderer = pillar.GetComponent<Renderer>();
        pillarRenderer.sharedMaterial = pillarMaterial;
        pillarRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pillarRenderer.receiveShadows = false;
        pillarRenderer.allowOcclusionWhenDynamic = false;

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "GroundGlow";
        ground.transform.SetParent(visualRoot, false);
        ground.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        ground.transform.localScale = new Vector3(groundRadius * 2f, 0.02f, groundRadius * 2f);
        DestroyCollider(ground);
        groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.sharedMaterial = groundMaterial;
        groundRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        groundRenderer.receiveShadows = false;
        groundRenderer.allowOcclusionWhenDynamic = false;

        if (addPointLight)
        {
            GameObject lightObject = new GameObject("BeaconLight");
            lightObject.transform.SetParent(visualRoot, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            pointLight = lightObject.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = pillarColor;
            pointLight.range = lightRange;
            pointLight.intensity = lightIntensity;
            pointLight.shadows = LightShadows.None;
        }

        UpdateVisibility();
    }

    private void ConfigureTriggerCollider()
    {
        if (!fitTriggerToBeacon)
            return;

        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
            return;

        float worldDiameter = Mathf.Max(pillarRadius * 2f, groundRadius * 2f) + triggerRadiusPadding * 2f;
        float worldCenterY = triggerBaseOffset + triggerHeight * 0.5f;

        Vector3 lossyScale = transform.lossyScale;
        triggerCollider.isTrigger = true;
        triggerCollider.center = new Vector3(
            0f,
            WorldToLocalLength(worldCenterY, lossyScale.y),
            0f);
        triggerCollider.size = new Vector3(
            WorldToLocalLength(worldDiameter, lossyScale.x),
            WorldToLocalLength(triggerHeight, lossyScale.y),
            WorldToLocalLength(worldDiameter, lossyScale.z));
    }

    private static void DestroyCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }

    private static void ConfigureTransparentMaterial(Material material)
    {
        if (material == null)
            return;

        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void TrySubscribeToTaskManager()
    {
        if (subscribedToTaskManager || TaskManager.Instance == null)
            return;

        TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
        TaskManager.Instance.OnTasksChanged += HandleTasksChanged;
        subscribedToTaskManager = true;
    }

    private void UnsubscribeFromTaskManager()
    {
        if (!subscribedToTaskManager || TaskManager.Instance == null)
            return;

        TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
        TaskManager.Instance.OnTasksChanged -= HandleTasksChanged;
        subscribedToTaskManager = false;
    }

    private string GetRelatedTaskId()
    {
        ObjectiveTriggerZone objectiveTrigger = GetComponent<ObjectiveTriggerZone>();
        if (objectiveTrigger != null && !string.IsNullOrEmpty(objectiveTrigger.taskId))
            return objectiveTrigger.taskId;

        GoalTrigger goalTrigger = GetComponent<GoalTrigger>();
        if (goalTrigger != null && !string.IsNullOrEmpty(goalTrigger.completeTaskId))
            return goalTrigger.completeTaskId;

        return string.Empty;
    }

    private static float WorldToLocalLength(float worldLength, float lossyScaleAxis)
    {
        float scale = Mathf.Abs(lossyScaleAxis);
        if (scale < 0.0001f)
            return worldLength;

        return worldLength / scale;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a short on-screen hint when the player hits a physical obstacle.
/// UI is created at runtime under the gameplay HUD panel when needed.
/// </summary>
public class ObstacleHitHintUI : MonoBehaviour
{
    public static ObstacleHitHintUI Instance { get; private set; }

    [Header("Display")]
    [SerializeField] private string messagePrefix = "Encontrarse con un obstaculo: ";
    [SerializeField] private float displayDuration = 2f;

    [Header("Optional References")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintText;

    private float hideAt = -1f;

    public static ObstacleHitHintUI EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        ObstacleHitHintUI existing = FindFirstObjectByType<ObstacleHitHintUI>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        var host = new GameObject("ObstacleHitHintUI");
        return host.AddComponent<ObstacleHitHintUI>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (hintPanel == null || !hintPanel.activeSelf)
            return;

        if (Time.time >= hideAt)
            hintPanel.SetActive(false);
    }

    public void Show(string message, string prefixOverride = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        EnsureUI();
        if (hintText == null)
            return;

        string effectivePrefix = prefixOverride ?? messagePrefix;
        hintText.text = string.IsNullOrEmpty(effectivePrefix)
            ? message
            : effectivePrefix + message;

        if (hintPanel != null)
            hintPanel.SetActive(true);

        hideAt = Time.time + displayDuration;
    }

    private Transform ResolveHudParent()
    {
        if (GameManager.Instance != null && GameManager.Instance.hudPanel != null)
            return GameManager.Instance.hudPanel.transform;

        GameObject hud = GameObject.Find("HUDPanel");
        return hud != null ? hud.transform : null;
    }

    private void EnsureUI()
    {
        if (hintText != null)
            return;

        Transform parent = ResolveHudParent();
        if (parent == null)
            return;

        var panelObject = new GameObject("ObstacleHitHint", typeof(RectTransform));
        panelObject.transform.SetParent(parent, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -120f);
        panelRect.sizeDelta = new Vector2(720f, 96f);

        Image background = panelObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0f, 0f, 0.72f);
        background.raycastTarget = false;

        var textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 10f);
        textRect.offsetMax = new Vector2(-20f, -10f);

        hintText = textObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            hintText.font = TMP_Settings.defaultFontAsset;

        hintText.fontSize = 34f;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = new Color(1f, 0.45f, 0.45f, 1f);
        hintText.raycastTarget = false;
        hintText.enableWordWrapping = true;

        hintPanel = panelObject;
        hintPanel.SetActive(false);
    }
}

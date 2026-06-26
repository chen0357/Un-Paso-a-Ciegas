using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DamageObject : MonoBehaviour
{
    private const string DefaultHitClipPath = "Assets/UI/Audio/shouji.mp3";

    public int damageAmount = 10;
    public float damageCooldown = 1f;

    [Header("Hit Hint")]
    [TextArea(1, 3)]

    public string hitHintMessage;

    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float hitVolume = 1f;

    private float lastDamageTime = -999f;
    private PlayerDamageReceiver currentReceiver;

    private static AudioClip s_defaultHitClip;
    private static AudioSource s_hitAudioSource;
    private static float s_lastHitFeedbackTime = -999f;

    private void Awake()
    {
        if (hitClip != null && s_defaultHitClip == null)
            s_defaultHitClip = hitClip;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (hitClip == null)
            hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultHitClipPath);
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        currentReceiver = receiver;

        if (GameManager.Instance != null && GameManager.Instance.CanProcessGameplay())
            GameManager.Instance.RegisterCollision();

        TryApplyDamage();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        currentReceiver = receiver;
        TryApplyDamage();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        if (receiver == currentReceiver)
        {
            currentReceiver = null;
        }
    }

    private void TryApplyDamage()
    {
        if (currentReceiver == null) return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;

        string source = transform.parent != null
            ? transform.parent.name + " / " + gameObject.name
            : gameObject.name;
        currentReceiver.ReceiveDamage(damageAmount, source);
        PlayHitFeedback();
    }

    private void PlayHitFeedback()
    {
        if (Time.time - s_lastHitFeedbackTime < damageCooldown)
            return;

        s_lastHitFeedbackTime = Time.time;
        PlayHitSound();
        ShowHitHint();
    }

    private string GetHitHintMessage()
    {
        if (!string.IsNullOrWhiteSpace(hitHintMessage))
            return hitHintMessage.Trim();

        Transform labelSource = transform.parent != null ? transform.parent : transform;
        return labelSource.name;
    }

    private void ShowHitHint()
    {
        string message = GetHitHintMessage();
        if (string.IsNullOrWhiteSpace(message))
            return;

        ObstacleHitHintUI ui = ObstacleHitHintUI.EnsureInstance();
        if (ui != null)
            ui.Show(message);
    }

    private void PlayHitSound()
    {
        AudioClip clip = hitClip != null ? hitClip : GetDefaultHitClip();
        if (clip == null)
            return;

        AudioSource source = GetSharedHitAudioSource();
        if (source == null)
            return;

        float volume = hitVolume;
        if (SettingsManager.Instance != null)
            volume *= SettingsManager.Instance.uiVolume;

        source.PlayOneShot(clip, volume);
    }

    private static AudioClip GetDefaultHitClip()
    {
        if (s_defaultHitClip != null)
            return s_defaultHitClip;

#if UNITY_EDITOR
        s_defaultHitClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultHitClipPath);
#endif
        return s_defaultHitClip;
    }

    private static AudioSource GetSharedHitAudioSource()
    {
        if (s_hitAudioSource != null)
            return s_hitAudioSource;

        var audioObject = new GameObject("ObstacleHitAudio");
        DontDestroyOnLoad(audioObject);

        s_hitAudioSource = audioObject.AddComponent<AudioSource>();
        s_hitAudioSource.playOnAwake = false;
        s_hitAudioSource.spatialBlend = 0f;

        return s_hitAudioSource;
    }
}
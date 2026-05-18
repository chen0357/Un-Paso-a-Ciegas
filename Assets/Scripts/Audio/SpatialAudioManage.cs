using UnityEngine;

public class SpatialAudioManager : MonoBehaviour
{
    [Header("Player")]
    public Transform playerHead;

    [Header("Goal Audio")]
    public Transform goalTarget;
    public AudioSource goalAudio;
    public float goalMaxDistance = 25f;
    public float goalMinVolume = 0.05f;
    public float goalMaxVolume = 1f;

    [Header("Danger Audio")]
    public Transform dangerTarget;
    public AudioSource dangerAudio;
    public float dangerRange = 12f;
    public float dangerMinVolume = 0.2f;
    public float dangerMaxVolume = 1f;
    public float dangerSlowInterval = 1.2f;
    public float dangerFastInterval = 0.15f;

    private float dangerTimer = 0f;

    private void Start()
    {
        Debug.Log("SpatialAudioManager Started");

        if (playerHead == null) Debug.LogError("playerHead 没有绑定");
        if (goalTarget == null) Debug.LogError("goalTarget 没有绑定");
        if (goalAudio == null) Debug.LogError("goalAudio 没有绑定");
        if (dangerTarget == null) Debug.LogError("dangerTarget 没有绑定");
        if (dangerAudio == null) Debug.LogError("dangerAudio 没有绑定");

        if (goalAudio != null && goalAudio.clip == null) Debug.LogError("goalAudio 没有 AudioClip");
        if (dangerAudio != null && dangerAudio.clip == null) Debug.LogError("dangerAudio 没有 AudioClip");
    }

    private void Update()
    {
        if (playerHead == null)
        {
            Debug.LogWarning("Update 停止：playerHead 是 null");
            return;
        }

        UpdateGoalAudio();
        UpdateDangerAudio();
    }

    private void UpdateGoalAudio()
    {
        if (goalTarget == null || goalAudio == null)
        {
            Debug.LogWarning("Goal 不触发：goalTarget 或 goalAudio 没绑定");
            return;
        }

        float distance = Vector3.Distance(playerHead.position, goalTarget.position);
        float closeness = 1f - Mathf.Clamp01(distance / goalMaxDistance);

        Debug.Log($"Goal Distance={distance}, Closeness={closeness}");

        if (distance > goalMaxDistance)
        {
            goalAudio.Stop();
            return;
        }

        goalAudio.volume = Mathf.Lerp(goalMinVolume, goalMaxVolume, closeness);

        if (!goalAudio.isPlaying)
        {
            Debug.Log("播放 Goal Audio");
            goalAudio.Play();
        }
    }

    private void UpdateDangerAudio()
    {
        if (dangerTarget == null || dangerAudio == null)
        {
            Debug.LogWarning("Danger 不触发：dangerTarget 或 dangerAudio 没绑定");
            return;
        }

        float distance = Vector3.Distance(playerHead.position, dangerTarget.position);

        Debug.Log($"Danger Distance={distance}, Range={dangerRange}");

        if (distance > dangerRange)
        {
            dangerTimer = 0f;
            dangerAudio.Stop();
            return;
        }

        float closeness = 1f - Mathf.Clamp01(distance / dangerRange);

        dangerAudio.volume = Mathf.Lerp(dangerMinVolume, dangerMaxVolume, closeness);

        float interval = Mathf.Lerp(dangerSlowInterval, dangerFastInterval, closeness);

        dangerTimer += Time.unscaledDeltaTime;

        if (dangerTimer >= interval)
        {
            Debug.Log("播放 Danger Audio");
            dangerAudio.Play();
            dangerTimer = 0f;
        }
    }
}
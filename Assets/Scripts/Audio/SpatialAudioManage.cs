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
        if (playerHead == null) Debug.LogError("SpatialAudioManager: playerHead is not assigned.");
        if (goalTarget == null) Debug.LogError("SpatialAudioManager: goalTarget is not assigned.");
        if (goalAudio == null) Debug.LogError("SpatialAudioManager: goalAudio is not assigned.");
        if (dangerTarget == null) Debug.LogError("SpatialAudioManager: dangerTarget is not assigned.");
        if (dangerAudio == null) Debug.LogError("SpatialAudioManager: dangerAudio is not assigned.");

        if (goalAudio != null && goalAudio.clip == null)
            Debug.LogError("SpatialAudioManager: goalAudio has no AudioClip.");
        if (dangerAudio != null && dangerAudio.clip == null)
            Debug.LogError("SpatialAudioManager: dangerAudio has no AudioClip.");
    }

    private void Update()
    {
        if (playerHead == null)
            return;

        UpdateGoalAudio();
        UpdateDangerAudio();
    }

    private void UpdateGoalAudio()
    {
        if (goalTarget == null || goalAudio == null)
            return;

        float distance = Vector3.Distance(playerHead.position, goalTarget.position);
        float closeness = 1f - Mathf.Clamp01(distance / goalMaxDistance);

        if (distance > goalMaxDistance)
        {
            goalAudio.Stop();
            return;
        }

        goalAudio.volume = Mathf.Lerp(goalMinVolume, goalMaxVolume, closeness);

        if (!goalAudio.isPlaying)
            goalAudio.Play();
    }

    private void UpdateDangerAudio()
    {
        if (dangerTarget == null || dangerAudio == null)
            return;

        float distance = Vector3.Distance(playerHead.position, dangerTarget.position);

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
            dangerAudio.Play();
            dangerTimer = 0f;
        }
    }
}

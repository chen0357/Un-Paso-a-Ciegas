using UnityEngine;

/// <summary>
/// One accessible pedestrian signal pole. Attach to each traffic_light instance.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TrafficLightSignal : MonoBehaviour
{
    public enum CrossingGroup
    {
        Unassigned = -1,
        GroupA = 0,
        GroupB = 1
    }

    [Tooltip("Which crossing direction this pole serves. Leave Unassigned to auto-detect from position.")]
    public CrossingGroup crossingGroup = CrossingGroup.Unassigned;

    [Header("Audio")]
    public float volume = 0.55f;
    public float maxDistance = 18f;

    private AudioSource audioSource;

    public AudioSource AudioSource => audioSource;

    private void Awake()
    {
        EnsureAudioSource();
        ConfigureAudioSource();
    }

    public void EnsureAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void ConfigureAudioSource()
    {
        EnsureAudioSource();
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
    }

    public void ApplyClip(AudioClip clip)
    {
        EnsureAudioSource();
        if (audioSource == null || clip == null)
            return;

        audioSource.clip = clip;
    }

    public void PlayLooping()
    {
        EnsureAudioSource();
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.loop = true;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void StopRing()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.loop = false;
    }
}

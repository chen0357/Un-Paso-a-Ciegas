using UnityEngine;

public class CaneAudioSystem : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Surface Clips")]
    public AudioClip groundClip;
    public AudioClip wallClip;
    public AudioClip obstacleClip;
    public AudioClip woodClip;
    public AudioClip metalClip;
    public AudioClip tactilePavingClip;
    public AudioClip defaultClip;

    [Header("Slide Clip")]
    public AudioClip slideClip;
    [SerializeField] private AudioSource slideAudioSource;

    [Header("Volume Settings")]
    public float baseVolume = 1f;
    public float minHitVolume = 0.2f;
    public float maxHitVolume = 1f;
    public float minSlideVolume = 0.3f;
    public float maxSlideVolume = 0.8f;

    private bool isSlidePlaying;

    private void Awake()
    {
        EnsureSlideAudioSource();
    }

    public void PlaySurfaceSound(SurfaceTag surfaceTag, float intensity)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("CaneAudioSystem: AudioSource is missing.");
            return;
        }

        AudioClip clip = ResolveClip(surfaceTag);
        if (clip == null)
            return;

        float hitVolume = Mathf.Lerp(minHitVolume, maxHitVolume, intensity);
        float finalVolume = baseVolume * hitVolume * GetSettingsVolumeMultiplier();

        audioSource.PlayOneShot(clip, finalVolume);
    }

    public void StartOrUpdateSlideSound(float intensity)
    {
        if (slideClip == null)
            return;

        EnsureSlideAudioSource();
        if (slideAudioSource == null)
            return;

        float slideVolume = Mathf.Lerp(minSlideVolume, maxSlideVolume, intensity);
        float finalVolume = baseVolume * slideVolume * GetSettingsVolumeMultiplier();
        slideAudioSource.volume = finalVolume;

        if (!isSlidePlaying)
        {
            slideAudioSource.clip = slideClip;
            slideAudioSource.loop = true;
            slideAudioSource.Play();
            isSlidePlaying = true;
        }
    }

    public void StopSlideSound()
    {
        if (!isSlidePlaying || slideAudioSource == null)
            return;

        slideAudioSource.Stop();
        slideAudioSource.clip = null;
        isSlidePlaying = false;
    }

    public bool IsSlidePlaying()
    {
        return isSlidePlaying;
    }

    private void EnsureSlideAudioSource()
    {
        if (slideAudioSource != null || slideClip == null)
            return;

        slideAudioSource = gameObject.AddComponent<AudioSource>();
        slideAudioSource.playOnAwake = false;
        slideAudioSource.loop = true;
        slideAudioSource.spatialBlend = audioSource != null ? audioSource.spatialBlend : 1f;
        slideAudioSource.minDistance = audioSource != null ? audioSource.minDistance : 0.3f;
        slideAudioSource.maxDistance = audioSource != null ? audioSource.maxDistance : 5f;
    }

    private AudioClip ResolveClip(SurfaceTag surfaceTag)
    {
        if (surfaceTag == null)
            return defaultClip;

        if (surfaceTag.customHitClip != null)
            return surfaceTag.customHitClip;

        return GetClipBySurface(surfaceTag.surfaceType);
    }

    private AudioClip GetClipBySurface(SurfaceType surfaceType)
    {
        switch (surfaceType)
        {
            case SurfaceType.Ground: return groundClip;
            case SurfaceType.Wall: return wallClip;
            case SurfaceType.Obstacle: return obstacleClip;
            case SurfaceType.Wood: return woodClip;
            case SurfaceType.Metal: return metalClip;
            case SurfaceType.TactilePaving: return tactilePavingClip;
            default: return defaultClip;
        }
    }

    private float GetSettingsVolumeMultiplier()
    {
        if (SettingsManager.Instance == null)
            return 1f;

        return SettingsManager.Instance.uiVolume;
    }
}

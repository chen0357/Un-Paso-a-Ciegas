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

    [Header("Volume Settings")]
    public float baseVolume = 1f;
    public float minHitVolume = 0.2f;
    public float maxHitVolume = 1f;

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
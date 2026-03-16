using UnityEngine;

public class CaneAudioSystem : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Surface Clips")]
    public AudioClip defaultClip;
    public AudioClip groundClip;
    public AudioClip wallClip;
    public AudioClip obstacleClip;
    public AudioClip woodClip;
    public AudioClip metalClip;
    public AudioClip tactileClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float defaultVolume = 1f;

    public void PlaySurfaceSound(SurfaceType surfaceType)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("CaneAudioSystem: AudioSource is missing.");
            return;
        }

        AudioClip clipToPlay = GetClipBySurface(surfaceType);

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, defaultVolume);
        }
    }

    private AudioClip GetClipBySurface(SurfaceType surfaceType)
    {
        switch (surfaceType)
        {
            case SurfaceType.Ground:
                return groundClip;

            case SurfaceType.Wall:
                return wallClip;

            case SurfaceType.Obstacle:
                return obstacleClip;

            case SurfaceType.Wood:
                return woodClip;

            case SurfaceType.Metal:
                return metalClip;

            case SurfaceType.TactilePaving:
                return tactileClip;

            default:
                return defaultClip;
        }
    }
}
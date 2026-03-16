using UnityEngine;

public class CaneDetector : MonoBehaviour
{
    [Header("References")]
    public CaneAudioSystem caneAudioSystem;
    public CaneHapticSystem caneHapticSystem;

    [Header("Debug")]
    public bool showDebugLog = true;

    private SurfaceType lastSurfaceType = SurfaceType.Default;
    private float lastHitTime = 0f;

    [Header("Hit Cooldown")]
    public float hitCooldown = 0.15f;

    private void OnTriggerEnter(Collider other)
    {
        DetectSurface(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time - lastHitTime > hitCooldown)
        {
            DetectSurface(other);
        }
    }

    private void DetectSurface(Collider other)
    {
        SurfaceTag surfaceTag = other.GetComponent<SurfaceTag>();

        SurfaceType currentSurface = SurfaceType.Default;

        if (surfaceTag != null)
        {
            currentSurface = surfaceTag.surfaceType;
        }

        lastSurfaceType = currentSurface;
        lastHitTime = Time.time;

        if (showDebugLog)
        {
            Debug.Log("Cane hit: " + other.gameObject.name + " | SurfaceType: " + currentSurface);
        }

        if (caneAudioSystem != null)
        {
            caneAudioSystem.PlaySurfaceSound(currentSurface);
        }

        if (caneHapticSystem != null)
        {
            caneHapticSystem.PlayHapticBySurface(currentSurface);
        }
    }
}
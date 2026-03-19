using System.Collections.Generic;
using UnityEngine;

public class CaneDetector : MonoBehaviour
{
    [Header("References")]
    public CaneAudioSystem caneAudioSystem;
    public CaneHapticSystem caneHapticSystem;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Header("Continuous Surface Cooldown")]
    public float continuousHitCooldown = 0.2f;

    [Header("Hit Intensity")]
    public float minHitSpeed = 0.1f;
    public float maxHitSpeed = 2.0f;

    private Dictionary<Collider, float> lastContinuousHitTime = new Dictionary<Collider, float>();
    private HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    private Vector3 lastPosition;
    private float currentHitIntensity = 0f;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        float speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        currentHitIntensity = Mathf.InverseLerp(minHitSpeed, maxHitSpeed, speed);
        currentHitIntensity = Mathf.Clamp01(currentHitIntensity);

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        SurfaceType surfaceType = GetSurfaceType(other);

        if (IsContinuousSurface(surfaceType))
        {
            DetectSurface(other, surfaceType, currentHitIntensity);
            lastContinuousHitTime[other] = Time.time;
        }
        else
        {
            if (triggeredColliders.Contains(other))
                return;

            triggeredColliders.Add(other);
            DetectSurface(other, surfaceType, currentHitIntensity);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        SurfaceType surfaceType = GetSurfaceType(other);

        if (!IsContinuousSurface(surfaceType))
            return;

        if (!lastContinuousHitTime.ContainsKey(other))
        {
            lastContinuousHitTime[other] = Time.time;
            DetectSurface(other, surfaceType, currentHitIntensity);
            return;
        }

        if (Time.time - lastContinuousHitTime[other] >= continuousHitCooldown)
        {
            DetectSurface(other, surfaceType, currentHitIntensity);
            lastContinuousHitTime[other] = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        triggeredColliders.Remove(other);
        lastContinuousHitTime.Remove(other);
    }

    private SurfaceType GetSurfaceType(Collider other)
    {
        SurfaceTag surfaceTag = other.GetComponent<SurfaceTag>();
        return surfaceTag != null ? surfaceTag.surfaceType : SurfaceType.Default;
    }

    private bool IsContinuousSurface(SurfaceType surfaceType)
    {
        switch (surfaceType)
        {
            case SurfaceType.Ground:
            case SurfaceType.TactilePaving:
                return true;
            default:
                return false;
        }
    }

    private void DetectSurface(Collider other, SurfaceType currentSurface, float intensity)
    {
        if (showDebugLog)
        {
            Debug.Log($"Cane hit: {other.gameObject.name} | SurfaceType: {currentSurface} | Intensity: {intensity:F2}");
        }

        if (caneAudioSystem != null)
        {
            caneAudioSystem.PlaySurfaceSound(currentSurface, intensity);
        }

        if (caneHapticSystem != null)
        {
            caneHapticSystem.PlayHapticBySurface(currentSurface, intensity);
        }
    }
}
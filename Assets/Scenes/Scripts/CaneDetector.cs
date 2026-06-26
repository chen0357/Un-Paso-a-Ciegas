using System.Collections.Generic;
using UnityEngine;

public class CaneDetector : MonoBehaviour
{
    [Header("References")]
    public CaneAudioSystem caneAudioSystem;
    public CaneHapticSystem caneHapticSystem;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Header("Slide Detection")]
    [Tooltip("Seconds in contact before switching from hit to slide audio.")]
    public float slideContactDelay = 0.35f;
    [Tooltip("Minimum cane speed to count as sliding on a surface.")]
    public float minSlideSpeed = 0.15f;

    [Header("Hit Intensity")]
    public float minHitSpeed = 0.1f;
    public float maxHitSpeed = 2.0f;

    private readonly Dictionary<Collider, float> contactStartTime = new Dictionary<Collider, float>();
    private readonly HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    private Vector3 lastPosition;
    private float currentHitIntensity;
    private float currentMoveSpeed;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        currentMoveSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        currentHitIntensity = Mathf.Clamp01(Mathf.InverseLerp(minHitSpeed, maxHitSpeed, currentMoveSpeed));
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        SurfaceType surfaceType = GetSurfaceType(other);
        contactStartTime[other] = Time.time;

        if (IsContinuousSurface(surfaceType))
        {
            PlayHitFeedback(other, surfaceType, currentHitIntensity);
            return;
        }

        if (triggeredColliders.Contains(other))
            return;

        triggeredColliders.Add(other);
        PlayHitFeedback(other, surfaceType, currentHitIntensity);
    }

    private void OnTriggerStay(Collider other)
    {
        SurfaceType surfaceType = GetSurfaceType(other);
        if (!IsContinuousSurface(surfaceType))
            return;

        if (!contactStartTime.ContainsKey(other))
            contactStartTime[other] = Time.time;

        UpdateSlideAudio();
    }

    private void OnTriggerExit(Collider other)
    {
        triggeredColliders.Remove(other);
        contactStartTime.Remove(other);
        UpdateSlideAudio();
    }

    private void UpdateSlideAudio()
    {
        if (caneAudioSystem == null)
            return;

        bool shouldSlide = false;
        float slideIntensity = 0f;

        foreach (KeyValuePair<Collider, float> contact in contactStartTime)
        {
            Collider collider = contact.Key;
            if (collider == null)
                continue;

            SurfaceType surfaceType = GetSurfaceType(collider);
            if (!IsContinuousSurface(surfaceType))
                continue;

            float contactDuration = Time.time - contact.Value;
            if (contactDuration >= slideContactDelay && currentMoveSpeed >= minSlideSpeed)
            {
                shouldSlide = true;
                slideIntensity = Mathf.Max(slideIntensity, currentHitIntensity);
            }
        }

        if (shouldSlide)
        {
            if (showDebugLog && !caneAudioSystem.IsSlidePlaying())
                Debug.Log($"Cane slide started | Speed: {currentMoveSpeed:F2}");

            caneAudioSystem.StartOrUpdateSlideSound(slideIntensity);
        }
        else
        {
            caneAudioSystem.StopSlideSound();
        }
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

    private void PlayHitFeedback(Collider other, SurfaceType currentSurface, float intensity)
    {
        if (showDebugLog)
        {
            Debug.Log($"Cane hit: {other.gameObject.name} | SurfaceType: {currentSurface} | Intensity: {intensity:F2}");
        }

        if (caneAudioSystem != null)
        {
            SurfaceTag surfaceTag = other.GetComponent<SurfaceTag>();
            caneAudioSystem.PlaySurfaceSound(surfaceTag, intensity);
        }

        if (caneHapticSystem != null)
        {
            caneHapticSystem.PlayHapticBySurface(currentSurface, intensity);
        }
    }
}

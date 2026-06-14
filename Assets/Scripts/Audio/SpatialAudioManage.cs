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
    public float dangerRange = 2f;
    public float dangerMinVolume = 0.2f;
    public float dangerMaxVolume = 1f;
    public float dangerSlowInterval = 2.5f;
    public float dangerFastInterval = 1.5f;
    public bool autoFindNearestDangerZone = true;

    private float dangerTimer = 0f;
    private DangerZone[] cachedDangerZones;
    private float nextDangerZoneRefreshTime = 0f;
    private AudioSource activeDangerAudio;
    private AudioSource lastDangerAudio;

    private void Start()
    {
        if (playerHead == null) Debug.LogError("SpatialAudioManager: playerHead is not assigned.");
        if (goalTarget == null) Debug.LogError("SpatialAudioManager: goalTarget is not assigned.");
        if (goalAudio == null) Debug.LogError("SpatialAudioManager: goalAudio is not assigned.");
        if (dangerAudio == null) Debug.LogError("SpatialAudioManager: dangerAudio is not assigned.");

        if (goalAudio != null && goalAudio.clip == null)
            Debug.LogWarning("SpatialAudioManager: goalAudio has no AudioClip assigned.");
        if (dangerAudio != null && dangerAudio.clip == null)
            Debug.LogWarning("SpatialAudioManager: dangerAudio has no AudioClip assigned (may use AudioResource).");

        ConfigureDangerAudioSource(dangerAudio);
    }

    private void Update()
    {
        if (playerHead == null)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
        {
            StopAllSpatialAudio();
            return;
        }

        UpdateGoalAudio();
        UpdateDangerAudio();
    }

    private void StopAllSpatialAudio()
    {
        if (goalAudio != null && goalAudio.isPlaying)
            goalAudio.Stop();

        StopDangerAudio();
        dangerTimer = 0f;
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
        DangerZone nearestZone = null;
        float distance = ResolveDangerBoundaryDistance(out nearestZone);
        activeDangerAudio = GetDangerAudioSource(nearestZone);

        if (activeDangerAudio == null)
            return;

        if (distance < 0f || distance > dangerRange)
        {
            StopDangerAudio();
            dangerTimer = 0f;
            return;
        }

        if (lastDangerAudio != null && lastDangerAudio != activeDangerAudio && lastDangerAudio.isPlaying)
            lastDangerAudio.Stop();

        lastDangerAudio = activeDangerAudio;

        float closeness = 1f - Mathf.Clamp01(distance / dangerRange);
        activeDangerAudio.volume = Mathf.Lerp(dangerMinVolume, dangerMaxVolume, closeness);

        float interval = Mathf.Lerp(dangerSlowInterval, dangerFastInterval, closeness);
        dangerTimer += Time.deltaTime;

        if (dangerTimer >= interval)
        {
            activeDangerAudio.Play();
            dangerTimer = 0f;
        }
    }

    private void StopDangerAudio()
    {
        if (activeDangerAudio != null && activeDangerAudio.isPlaying)
            activeDangerAudio.Stop();

        if (lastDangerAudio != null && lastDangerAudio.isPlaying)
            lastDangerAudio.Stop();
    }

    private float ResolveDangerBoundaryDistance(out DangerZone nearestZone)
    {
        nearestZone = null;

        if (!autoFindNearestDangerZone)
        {
            nearestZone = dangerTarget != null ? dangerTarget.GetComponent<DangerZone>() : null;
            return GetDangerDistance(nearestZone, dangerTarget);
        }

        if (Time.time >= nextDangerZoneRefreshTime || cachedDangerZones == null)
        {
            cachedDangerZones = FindObjectsByType<DangerZone>(FindObjectsSortMode.None);
            nextDangerZoneRefreshTime = Time.time + 1f;
        }

        float nearestDistance = float.MaxValue;

        if (cachedDangerZones != null)
        {
            for (int i = 0; i < cachedDangerZones.Length; i++)
            {
                DangerZone zone = cachedDangerZones[i];
                if (zone == null || !zone.isActiveAndEnabled)
                    continue;

                float distance = GetDangerDistance(zone, zone.transform);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestZone = zone;
                }
            }
        }

        if (nearestZone != null)
            return nearestDistance;

        nearestZone = dangerTarget != null ? dangerTarget.GetComponent<DangerZone>() : null;
        return GetDangerDistance(nearestZone, dangerTarget);
    }

    private float GetDangerDistance(DangerZone zone, Transform fallbackTransform)
    {
        if (playerHead == null)
            return -1f;

        Collider zoneCollider = zone != null ? zone.GetComponent<Collider>() : null;
        if (zoneCollider != null)
            return GetHorizontalBoundaryDistance(playerHead.position, zoneCollider.bounds);

        if (fallbackTransform != null)
            return Vector3.Distance(playerHead.position, fallbackTransform.position);

        return -1f;
    }

    private static float GetHorizontalBoundaryDistance(Vector3 playerPos, Bounds bounds)
    {
        float dx = 0f;
        if (playerPos.x < bounds.min.x)
            dx = bounds.min.x - playerPos.x;
        else if (playerPos.x > bounds.max.x)
            dx = playerPos.x - bounds.max.x;

        float dz = 0f;
        if (playerPos.z < bounds.min.z)
            dz = bounds.min.z - playerPos.z;
        else if (playerPos.z > bounds.max.z)
            dz = playerPos.z - bounds.max.z;

        // Inside the zone horizontally: treat as distance 0 so alarm always plays.
        if (dx == 0f && dz == 0f)
            return 0f;

        if (dx > 0f && dz > 0f)
            return Mathf.Sqrt(dx * dx + dz * dz);

        return Mathf.Max(dx, dz);
    }

    private AudioSource GetDangerAudioSource(DangerZone zone)
    {
        if (zone != null)
        {
            AudioSource zoneAudio = zone.GetComponent<AudioSource>();
            if (zoneAudio != null)
            {
                ConfigureDangerAudioSource(zoneAudio);
                return zoneAudio;
            }
        }

        ConfigureDangerAudioSource(dangerAudio);
        return dangerAudio;
    }

    private static void ConfigureDangerAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        source.spatialBlend = 0f;
    }
}

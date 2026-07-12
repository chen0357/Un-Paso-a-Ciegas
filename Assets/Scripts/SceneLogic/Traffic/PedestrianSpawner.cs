using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns pedestrians on one or more sidewalk lanes. Each lane needs its own spawn/despawn
/// transforms facing the walking direction.
/// </summary>
public class PedestrianSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Randomly picks one prefab from this list on each spawn.")]
    public List<GameObject> pedestrianPrefabs = new List<GameObject>();

    [Tooltip("Used when Pedestrian Prefabs is empty.")]
    public GameObject pedestrianPrefab;

    [Header("Lanes")]
    public List<TrafficLaneConfig> lanes = new List<TrafficLaneConfig>();

    [Header("Defaults (shared by all lanes)")]
    public float spawnInterval = 6f;
    public float initialDelay = 2f;
    [Tooltip("Each pedestrian gets a random speed in this range (m/s).")]
    public float minSpeed = 1f;
    public float maxSpeed = 3.2f;
    public int maxActivePedestrians = 6;

    [Tooltip("Random left/right offset from the spawn line (metres). 0 = exact lane centre.")]
    public float lateralOffsetRange = 0.75f;

    [Header("Legacy single lane")]
    public Transform spawnPoint;
    public Transform despawnPoint;

    [Header("Traffic Light Link")]
    [Tooltip("North-south sidewalk pedestrians cross east-west traffic at the intersection.")]
    public TrafficFlowAxis flowAxis;

    [Tooltip("Extra wait after the crossing signal starts before pedestrians enter the intersection.")]
    public float releaseDelay = 0.5f;

    private bool crossingBlocked;
    private Bounds stopBounds;
    private float releaseHoldUntil;
    private readonly List<PedestrianWalker> activeWalkers = new List<PedestrianWalker>();
    private readonly List<LaneRuntime> runtimeLanes = new List<LaneRuntime>();

    public bool IsCrossingBlocked => crossingBlocked;
    public bool IsCrossingClosingSoon => crossingClosingSoon;
    public bool IsReleaseHoldActive => !crossingBlocked && Time.time < releaseHoldUntil;
    public Bounds StopBounds => stopBounds;

    public void SetCrossingBlocked(bool blocked, Bounds bounds)
    {
        SetCrossingState(blocked, false, bounds);
    }

    public void SetCrossingState(bool blocked, bool closingSoon, Bounds bounds)
    {
        bool wasBlocked = crossingBlocked;
        crossingBlocked = blocked;
        crossingClosingSoon = closingSoon;
        stopBounds = bounds;

        if (wasBlocked && !blocked)
            releaseHoldUntil = Time.time + releaseDelay;
        else if (blocked)
            releaseHoldUntil = 0f;
    }

    private bool crossingClosingSoon;

    internal void RegisterWalker(PedestrianWalker walker)
    {
        if (walker == null || activeWalkers.Contains(walker))
            return;

        activeWalkers.Add(walker);
    }

    internal void UnregisterWalker(PedestrianWalker walker)
    {
        if (walker == null)
            return;

        activeWalkers.Remove(walker);
    }

    public PedestrianWalker FindWalkerAhead(PedestrianWalker self, int laneIndex)
    {
        if (self == null)
            return null;

        PruneActiveWalkers();

        PedestrianWalker closestAhead = null;
        float closestGap = float.MaxValue;
        float selfAxis = self.GetTravelAxis();

        for (int i = 0; i < activeWalkers.Count; i++)
        {
            PedestrianWalker other = activeWalkers[i];
            if (other == null || other == self || other.LaneIndex != laneIndex)
                continue;

            float gap = other.GetTravelAxis() - selfAxis;
            if (gap <= StopLineTolerance || gap >= closestGap)
                continue;

            closestGap = gap;
            closestAhead = other;
        }

        return closestAhead;
    }

    internal bool IsLaneSpawnBlocked(int laneIndex, Transform laneSpawnPoint, float clearDistance)
    {
        if (laneSpawnPoint == null)
            return false;

        PruneActiveWalkers();

        Vector3 laneForward = laneSpawnPoint.forward.normalized;
        float spawnAxis = Vector3.Dot(laneSpawnPoint.position, laneForward);

        for (int i = 0; i < activeWalkers.Count; i++)
        {
            PedestrianWalker walker = activeWalkers[i];
            if (walker == null || walker.LaneIndex != laneIndex)
                continue;

            float gap = Vector3.Dot(walker.transform.position, laneForward) - spawnAxis;
            if (gap >= 0f && gap < clearDistance)
                return true;
        }

        return false;
    }

    private void PruneActiveWalkers()
    {
        for (int i = activeWalkers.Count - 1; i >= 0; i--)
        {
            if (activeWalkers[i] == null)
                activeWalkers.RemoveAt(i);
        }
    }

    private const float StopLineTolerance = 0.05f;

    private class LaneRuntime
    {
        public TrafficLaneConfig config;
        public float timer;
        public int activePedestrians;
    }

    private void Start()
    {
        BuildRuntimeLanes();
    }

    private void BuildRuntimeLanes()
    {
        runtimeLanes.Clear();

        List<TrafficLaneConfig> sourceLanes = ResolveLaneConfigs();
        for (int i = 0; i < sourceLanes.Count; i++)
        {
            TrafficLaneConfig lane = sourceLanes[i];
            if (lane == null || lane.spawnPoint == null || lane.despawnPoint == null)
                continue;

            runtimeLanes.Add(new LaneRuntime
            {
                config = lane,
                timer = ResolveInitialDelay(lane, i)
            });
        }
    }

    private List<TrafficLaneConfig> ResolveLaneConfigs()
    {
        if (lanes != null && lanes.Count > 0)
            return lanes;

        if (spawnPoint != null && despawnPoint != null)
        {
            return new List<TrafficLaneConfig>
            {
                new TrafficLaneConfig
                {
                    laneName = "Lane 1",
                    spawnPoint = spawnPoint,
                    despawnPoint = despawnPoint
                }
            };
        }

        return new List<TrafficLaneConfig>();
    }

    private float ResolveInitialDelay(TrafficLaneConfig lane, int laneIndex)
    {
        if (lane.initialDelay > 0f)
            return lane.initialDelay;

        return initialDelay + laneIndex * 1.25f;
    }

    private void Update()
    {
        if (!HasAnyPedestrianPrefab() || runtimeLanes.Count == 0)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        for (int i = 0; i < runtimeLanes.Count; i++)
            UpdateLane(runtimeLanes[i], i);
    }

    private void UpdateLane(LaneRuntime lane, int laneIndex)
    {
        int laneLimit = ResolveMaxActivePedestrians(lane.config);
        if (lane.activePedestrians >= laneLimit)
            return;

        float spawnClearDistance = ResolveSpawnClearDistance();
        if (IsLaneSpawnBlocked(laneIndex, lane.config.spawnPoint, spawnClearDistance))
            return;

        lane.timer -= Time.deltaTime;
        if (lane.timer > 0f)
            return;

        lane.timer = ResolveSpawnInterval(lane.config);
        SpawnPedestrian(lane, laneIndex);
    }

    private void SpawnPedestrian(LaneRuntime lane, int laneIndex)
    {
        GameObject prefab = PickRandomPedestrianPrefab();
        if (prefab == null)
            return;

        Transform spawn = lane.config.spawnPoint;
        Transform despawn = lane.config.despawnPoint;

        GameObject pedestrian = Instantiate(prefab, spawn.position, Quaternion.identity);
        lane.activePedestrians++;

        PedestrianWalker mover = pedestrian.GetComponent<PedestrianWalker>();
        if (mover == null)
            mover = pedestrian.AddComponent<PedestrianWalker>();

        float min = ResolveMinSpeed(lane.config);
        float max = ResolveMaxSpeed(lane.config);
        if (max < min)
            max = min;

        float walkSpeed = Random.Range(min, max);
        float lateralOffset = lateralOffsetRange > 0f
            ? Random.Range(-lateralOffsetRange, lateralOffsetRange)
            : 0f;
        mover.Initialize(spawn, despawn, walkSpeed, laneIndex, lateralOffset);
        mover.BindSpawner(this);
        RegisterWalker(mover);

        PedestrianTracker tracker = pedestrian.GetComponent<PedestrianTracker>();
        if (tracker == null)
            tracker = pedestrian.AddComponent<PedestrianTracker>();
        tracker.spawner = this;
        tracker.laneIndex = laneIndex;
        tracker.walker = mover;
    }

    public void NotifyPedestrianDestroyed(int laneIndex, PedestrianWalker walker)
    {
        UnregisterWalker(walker);

        if (laneIndex < 0 || laneIndex >= runtimeLanes.Count)
            return;

        runtimeLanes[laneIndex].activePedestrians =
            Mathf.Max(0, runtimeLanes[laneIndex].activePedestrians - 1);
    }

    private float ResolveSpawnInterval(TrafficLaneConfig lane)
    {
        return lane.spawnInterval > 0f ? lane.spawnInterval : spawnInterval;
    }

    private float ResolveMinSpeed(TrafficLaneConfig lane)
    {
        if (lane.speed > 0f)
            return Mathf.Max(0.5f, lane.speed * 0.6f);

        return minSpeed;
    }

    private float ResolveMaxSpeed(TrafficLaneConfig lane)
    {
        if (lane.speed > 0f)
            return lane.speed * 1.4f;

        return maxSpeed;
    }

    private int ResolveMaxActivePedestrians(TrafficLaneConfig lane)
    {
        return lane.maxActiveCars > 0 ? lane.maxActiveCars : maxActivePedestrians;
    }

    private bool HasAnyPedestrianPrefab()
    {
        return GetPedestrianPrefabPool().Count > 0;
    }

    private List<GameObject> GetPedestrianPrefabPool()
    {
        var pool = new List<GameObject>();

        if (pedestrianPrefabs != null)
        {
            for (int i = 0; i < pedestrianPrefabs.Count; i++)
            {
                GameObject prefab = pedestrianPrefabs[i];
                if (prefab != null && !pool.Contains(prefab))
                    pool.Add(prefab);
            }
        }

        if (pool.Count == 0 && pedestrianPrefab != null)
            pool.Add(pedestrianPrefab);

        return pool;
    }

    private GameObject PickRandomPedestrianPrefab()
    {
        List<GameObject> pool = GetPedestrianPrefabPool();
        if (pool.Count == 0)
            return null;

        return pool[Random.Range(0, pool.Count)];
    }

    private float ResolveSpawnClearDistance()
    {
        List<GameObject> pool = GetPedestrianPrefabPool();
        if (pool.Count == 0)
            return 2.5f;

        float maxDistance = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            PedestrianWalker template = pool[i].GetComponent<PedestrianWalker>();
            if (template == null)
                continue;

            maxDistance = Mathf.Max(maxDistance, template.followDistance * 1.5f);
        }

        return maxDistance > 0f ? maxDistance : 2.5f;
    }

    private void OnDrawGizmosSelected()
    {
        List<TrafficLaneConfig> sourceLanes = ResolveLaneConfigs();
        for (int i = 0; i < sourceLanes.Count; i++)
        {
            TrafficLaneConfig lane = sourceLanes[i];
            if (lane == null || lane.spawnPoint == null || lane.despawnPoint == null)
                continue;

            Gizmos.color = Color.HSVToRGB((i * 0.17f + 0.5f) % 1f, 0.75f, 0.95f);
            Gizmos.DrawWireSphere(lane.spawnPoint.position, 0.45f);
            Gizmos.DrawRay(lane.spawnPoint.position, lane.spawnPoint.forward * 2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lane.despawnPoint.position, 0.35f);
        }
    }
}

/// <summary>
/// Notifies the spawner when a pedestrian is destroyed so the active count stays accurate.
/// </summary>
public class PedestrianTracker : MonoBehaviour
{
    [HideInInspector] public PedestrianSpawner spawner;
    [HideInInspector] public PedestrianWalker walker;
    [HideInInspector] public int laneIndex = -1;

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyPedestrianDestroyed(laneIndex, walker);
    }
}

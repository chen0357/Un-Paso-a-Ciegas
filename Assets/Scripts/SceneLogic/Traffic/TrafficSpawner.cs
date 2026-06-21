using System.Collections.Generic;
using UnityEngine;

public enum TrafficFlowAxis
{
    NorthSouth,
    EastWest
}

/// <summary>
/// Spawns traffic on one or more parallel lanes. Each lane needs its own spawn/despawn
/// transforms facing the driving direction.
/// </summary>
public class TrafficSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Randomly picks one prefab from this list on each spawn.")]
    public List<GameObject> carPrefabs = new List<GameObject>();

    [Tooltip("Used when Car Prefabs is empty.")]
    public GameObject carPrefab;

    [Header("Lanes")]
    public List<TrafficLaneConfig> lanes = new List<TrafficLaneConfig>();

    [Header("Defaults (shared by all lanes)")]
    public float spawnInterval = 4f;
    public float initialDelay = 1f;
    public float speed = 8f;
    public float speedVariation = 2f;
    public int maxActiveCars = 4;

    [Header("Legacy single lane")]
    public Transform spawnPoint;
    public Transform despawnPoint;

    [Header("Traffic Light Link")]
    public TrafficFlowAxis flowAxis;

    [Tooltip("Extra wait after the light turns green before queued cars start moving.")]
    public float releaseDelay = 1f;

    private bool crossingBlocked;
    private Bounds stopBounds;
    private float releaseHoldUntil;
    private readonly List<TrafficCar> activeCars = new List<TrafficCar>();
    private readonly List<LaneRuntime> runtimeLanes = new List<LaneRuntime>();

    public bool IsCrossingBlocked => crossingBlocked;
    public bool IsReleaseHoldActive => !crossingBlocked && Time.time < releaseHoldUntil;
    public Bounds StopBounds => stopBounds;

    public void SetCrossingBlocked(bool blocked, Bounds bounds)
    {
        bool wasBlocked = crossingBlocked;
        crossingBlocked = blocked;
        stopBounds = bounds;

        if (wasBlocked && !blocked)
            releaseHoldUntil = Time.time + releaseDelay;
        else if (blocked)
            releaseHoldUntil = 0f;
    }

    internal void RegisterCar(TrafficCar car)
    {
        if (car == null || activeCars.Contains(car))
            return;

        activeCars.Add(car);
    }

    internal void UnregisterCar(TrafficCar car)
    {
        if (car == null)
            return;

        activeCars.Remove(car);
    }

    public TrafficCar FindCarAhead(TrafficCar self, int laneIndex)
    {
        if (self == null)
            return null;

        PruneActiveCars();

        TrafficCar closestAhead = null;
        float closestGap = float.MaxValue;
        float selfAxis = self.GetTravelAxis();

        for (int i = 0; i < activeCars.Count; i++)
        {
            TrafficCar other = activeCars[i];
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

    internal bool IsLaneSpawnBlocked(int laneIndex, Transform spawnPoint, float clearDistance)
    {
        if (spawnPoint == null)
            return false;

        PruneActiveCars();

        Vector3 laneForward = spawnPoint.forward.normalized;
        float spawnAxis = Vector3.Dot(spawnPoint.position, laneForward);

        for (int i = 0; i < activeCars.Count; i++)
        {
            TrafficCar car = activeCars[i];
            if (car == null || car.LaneIndex != laneIndex)
                continue;

            float gap = Vector3.Dot(car.transform.position, laneForward) - spawnAxis;
            if (gap >= 0f && gap < clearDistance)
                return true;
        }

        return false;
    }

    private void PruneActiveCars()
    {
        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            if (activeCars[i] == null)
                activeCars.RemoveAt(i);
        }
    }

    private const float StopLineTolerance = 0.05f;

    private class LaneRuntime
    {
        public TrafficLaneConfig config;
        public float timer;
        public int activeCars;
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

        return initialDelay + laneIndex * 0.75f;
    }

    private void Update()
    {
        if (!HasAnyCarPrefab() || runtimeLanes.Count == 0)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        for (int i = 0; i < runtimeLanes.Count; i++)
            UpdateLane(runtimeLanes[i], i);
    }

    private void UpdateLane(LaneRuntime lane, int laneIndex)
    {
        if (crossingBlocked || IsReleaseHoldActive)
            return;

        int laneLimit = ResolveMaxActiveCars(lane.config);
        if (lane.activeCars >= laneLimit)
            return;

        float spawnClearDistance = ResolveSpawnClearDistance();
        if (IsLaneSpawnBlocked(laneIndex, lane.config.spawnPoint, spawnClearDistance))
            return;

        lane.timer -= Time.deltaTime;
        if (lane.timer > 0f)
            return;

        lane.timer = ResolveSpawnInterval(lane.config);
        SpawnCar(lane, laneIndex);
    }

    private void SpawnCar(LaneRuntime lane, int laneIndex)
    {
        GameObject prefab = PickRandomCarPrefab();
        if (prefab == null)
            return;

        Transform spawn = lane.config.spawnPoint;
        Transform despawn = lane.config.despawnPoint;

        GameObject car = Instantiate(prefab, spawn.position, Quaternion.identity);
        lane.activeCars++;

        TrafficCar mover = car.GetComponent<TrafficCar>();
        if (mover == null)
            mover = car.AddComponent<TrafficCar>();

        float baseSpeed = ResolveSpeed(lane.config);
        float carSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        mover.Initialize(spawn, despawn, carSpeed, laneIndex);
        mover.BindSpawner(this);
        RegisterCar(mover);

        TrafficCarTracker tracker = car.GetComponent<TrafficCarTracker>();
        if (tracker == null)
            tracker = car.AddComponent<TrafficCarTracker>();
        tracker.spawner = this;
        tracker.laneIndex = laneIndex;
        tracker.car = mover;
    }

    public void NotifyCarDestroyed(int laneIndex, TrafficCar car)
    {
        UnregisterCar(car);

        if (laneIndex < 0 || laneIndex >= runtimeLanes.Count)
            return;

        runtimeLanes[laneIndex].activeCars = Mathf.Max(0, runtimeLanes[laneIndex].activeCars - 1);
    }

    private float ResolveSpawnInterval(TrafficLaneConfig lane)
    {
        return lane.spawnInterval > 0f ? lane.spawnInterval : spawnInterval;
    }

    private float ResolveSpeed(TrafficLaneConfig lane)
    {
        return lane.speed > 0f ? lane.speed : speed;
    }

    private int ResolveMaxActiveCars(TrafficLaneConfig lane)
    {
        return lane.maxActiveCars > 0 ? lane.maxActiveCars : maxActiveCars;
    }

    private bool HasAnyCarPrefab()
    {
        return GetCarPrefabPool().Count > 0;
    }

    private List<GameObject> GetCarPrefabPool()
    {
        var pool = new List<GameObject>();

        if (carPrefabs != null)
        {
            for (int i = 0; i < carPrefabs.Count; i++)
            {
                GameObject prefab = carPrefabs[i];
                if (prefab != null && !pool.Contains(prefab))
                    pool.Add(prefab);
            }
        }

        if (pool.Count == 0 && carPrefab != null)
            pool.Add(carPrefab);

        return pool;
    }

    private GameObject PickRandomCarPrefab()
    {
        List<GameObject> pool = GetCarPrefabPool();
        if (pool.Count == 0)
            return null;

        return pool[Random.Range(0, pool.Count)];
    }

    private float ResolveSpawnClearDistance()
    {
        List<GameObject> pool = GetCarPrefabPool();
        if (pool.Count == 0)
            return 6f;

        float maxDistance = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            TrafficCar template = pool[i].GetComponent<TrafficCar>();
            if (template == null)
                continue;

            maxDistance = Mathf.Max(maxDistance, template.followDistance * 1.5f);
        }

        return maxDistance > 0f ? maxDistance : 6f;
    }

    private void OnDrawGizmosSelected()
    {
        List<TrafficLaneConfig> sourceLanes = ResolveLaneConfigs();
        for (int i = 0; i < sourceLanes.Count; i++)
        {
            TrafficLaneConfig lane = sourceLanes[i];
            if (lane == null || lane.spawnPoint == null || lane.despawnPoint == null)
                continue;

            Gizmos.color = Color.HSVToRGB((i * 0.17f) % 1f, 0.85f, 0.95f);
            Gizmos.DrawWireSphere(lane.spawnPoint.position, 0.9f);
            Gizmos.DrawRay(lane.spawnPoint.position, lane.spawnPoint.forward * 3f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lane.despawnPoint.position, 0.7f);
        }
    }
}

/// <summary>
/// Notifies the spawner when a car is destroyed so the active count stays accurate.
/// </summary>
public class TrafficCarTracker : MonoBehaviour
{
    [HideInInspector] public TrafficSpawner spawner;
    [HideInInspector] public TrafficCar car;
    [HideInInspector] public int laneIndex = -1;

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyCarDestroyed(laneIndex, car);
    }
}

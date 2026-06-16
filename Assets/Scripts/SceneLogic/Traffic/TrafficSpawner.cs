using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns traffic on one or more parallel lanes. Each lane needs its own spawn/despawn
/// transforms facing the driving direction.
/// </summary>
public class TrafficSpawner : MonoBehaviour
{
    [Header("Prefabs")]
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

    private readonly List<LaneRuntime> runtimeLanes = new List<LaneRuntime>();

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
        if (carPrefab == null || runtimeLanes.Count == 0)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        for (int i = 0; i < runtimeLanes.Count; i++)
            UpdateLane(runtimeLanes[i], i);
    }

    private void UpdateLane(LaneRuntime lane, int laneIndex)
    {
        int laneLimit = ResolveMaxActiveCars(lane.config);
        if (lane.activeCars >= laneLimit)
            return;

        lane.timer -= Time.deltaTime;
        if (lane.timer > 0f)
            return;

        lane.timer = ResolveSpawnInterval(lane.config);
        SpawnCar(lane, laneIndex);
    }

    private void SpawnCar(LaneRuntime lane, int laneIndex)
    {
        Transform spawn = lane.config.spawnPoint;
        Transform despawn = lane.config.despawnPoint;

        GameObject car = Instantiate(carPrefab, spawn.position, spawn.rotation);
        lane.activeCars++;

        TrafficCar mover = car.GetComponent<TrafficCar>();
        if (mover == null)
            mover = car.AddComponent<TrafficCar>();

        float baseSpeed = ResolveSpeed(lane.config);
        float carSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        mover.Initialize(spawn, despawn, carSpeed);

        TrafficCarTracker tracker = car.GetComponent<TrafficCarTracker>();
        if (tracker == null)
            tracker = car.AddComponent<TrafficCarTracker>();
        tracker.spawner = this;
        tracker.laneIndex = laneIndex;
    }

    public void NotifyCarDestroyed(int laneIndex)
    {
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
    [HideInInspector] public int laneIndex = -1;

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyCarDestroyed(laneIndex);
    }
}

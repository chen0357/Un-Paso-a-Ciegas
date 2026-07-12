using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alternates two groups of four pedestrian signals. Only the active group plays
/// the crossing cue clip, helping blind players know when it is safe to cross.
/// </summary>
public class TrafficLightController : MonoBehaviour
{
    [Header("Signals")]
    [Tooltip("Optional root that contains traffic_light_* instances. Auto-finds child Traffic_light if empty.")]
    public Transform signalsRoot;

    [Tooltip("Optional manual list. If empty, traffic_light_* objects are discovered under Signals Root.")]
    public List<TrafficLightSignal> signals = new List<TrafficLightSignal>();

    [Tooltip("Center of the intersection used for auto-grouping.")]
    public Vector3 intersectionCenter = new Vector3(-2.2f, 0f, -0.6f);

    [Header("Intersection Stop Zone")]
    [Tooltip("World-space corner where cars stop before entering the crossing (x, y, z).")]
    public Vector3 intersectionMin = new Vector3(-19f, 0f, -19f);

    [Tooltip("World-space opposite corner of the crossing stop zone (x, y, z).")]
    public Vector3 intersectionMax = new Vector3(16f, 0f, 15f);

    [Header("Pedestrian Stop Zone")]
    [Tooltip("When enabled, pedestrians wait at Pedestrian Intersection Min/Max instead of the vehicle stop zone.")]
    public bool useSeparatePedestrianStopZone = true;

    [Tooltip("World-space corner where pedestrians wait before crossing.")]
    public Vector3 pedestrianIntersectionMin = new Vector3(-19f, 0f, -19f);

    [Tooltip("World-space opposite corner of the pedestrian wait zone.")]
    public Vector3 pedestrianIntersectionMax = new Vector3(19f, 0f, 19f);

    [Tooltip("Optional. Seconds before a signal ends when waiting pedestrians stop entering. Use 0 to rely on Switch Pause instead.")]
    public float pedestrianCrossingLeadTime = 0f;

    [Header("Timing")]
    [Tooltip("How long each crossing signal rings before the other direction starts.")]
    public float playbackDuration = 18f;

    [Tooltip("Gap after ringing stops before the next direction rings. Perpendicular traffic stays held so pedestrians can finish crossing.")]
    public float switchPause = 8f;

    [Header("Audio")]
    public AudioClip ringClip;

    [Header("Traffic Link")]
    [Tooltip("North-south traffic stops while group A rings, and stays held until group B rings.")]
    public TrafficSpawner northSouthSpawner;

    [Tooltip("East-west traffic stops while group B rings, and stays held until group A rings.")]
    public TrafficSpawner eastWestSpawner;

    public bool autoFindSpawners = true;

    [Header("Pedestrian Link")]
    [Tooltip("North-south sidewalk pedestrians. They cross when group B is active (east-west traffic stopped).")]
    public PedestrianSpawner northSouthPedSpawner;

    [Tooltip("East-west sidewalk pedestrians. They cross when group A is active (north-south traffic stopped).")]
    public PedestrianSpawner eastWestPedSpawner;

    public bool autoFindPedSpawners = true;

    private readonly List<TrafficLightSignal> groupA = new List<TrafficLightSignal>();
    private readonly List<TrafficLightSignal> groupB = new List<TrafficLightSignal>();

    private int activeGroupIndex;
    private float phaseTimer;
    private float pauseTimer;
    private bool isPausedBetweenPhases;
    private bool pauseFollowsGroupA;

    private void Start()
    {
        if (autoFindSpawners)
            ResolveSpawners();

        if (autoFindPedSpawners)
            ResolvePedSpawners();

        DiscoverSignals();
        AssignGroups();
        PrepareSignals();
        BeginPhase(0);
    }

    private void Update()
    {
        if (!CanRun())
        {
            StopAllSignals();
            ApplyTrafficState();
            return;
        }

        if (isPausedBetweenPhases)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPausedBetweenPhases = false;
                BeginPhase(activeGroupIndex);
            }
            else
            {
                ApplyTrafficState();
            }

            return;
        }

        phaseTimer -= Time.deltaTime;
        if (phaseTimer <= 0f)
            SwitchToNextPhase();
    }

    private bool CanRun()
    {
        return GameManager.Instance == null || GameManager.Instance.CanProcessGameplay();
    }

    private void ResolveSpawners()
    {
        TrafficSpawner[] spawners = FindObjectsByType<TrafficSpawner>(FindObjectsSortMode.None);
        for (int i = 0; i < spawners.Length; i++)
        {
            TrafficSpawner spawner = spawners[i];
            if (spawner == null)
                continue;

            if (northSouthSpawner == null && spawner.flowAxis == TrafficFlowAxis.NorthSouth)
                northSouthSpawner = spawner;
            else if (eastWestSpawner == null && spawner.flowAxis == TrafficFlowAxis.EastWest)
                eastWestSpawner = spawner;
        }
    }

    private void ResolvePedSpawners()
    {
        PedestrianSpawner[] spawners = FindObjectsByType<PedestrianSpawner>(FindObjectsSortMode.None);
        for (int i = 0; i < spawners.Length; i++)
        {
            PedestrianSpawner spawner = spawners[i];
            if (spawner == null)
                continue;

            if (northSouthPedSpawner == null && spawner.flowAxis == TrafficFlowAxis.NorthSouth)
                northSouthPedSpawner = spawner;
            else if (eastWestPedSpawner == null && spawner.flowAxis == TrafficFlowAxis.EastWest)
                eastWestPedSpawner = spawner;
        }
    }

    private void DiscoverSignals()
    {
        if (signals.Count > 0)
            return;

        Transform root = ResolveSignalsRoot();
        TrafficLightSignal[] existing = root.GetComponentsInChildren<TrafficLightSignal>(true);
        for (int i = 0; i < existing.Length; i++)
        {
            TrafficLightSignal signal = existing[i];
            if (signal != null && IsTrafficLightInstance(signal.gameObject.name))
                signals.Add(signal);
        }

        if (signals.Count > 0)
            return;

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == root || !IsTrafficLightInstance(candidate.name))
                continue;

            TrafficLightSignal signal = candidate.GetComponent<TrafficLightSignal>();
            if (signal == null)
                signal = candidate.gameObject.AddComponent<TrafficLightSignal>();

            if (!signals.Contains(signal))
                signals.Add(signal);
        }

        if (signals.Count == 0)
            Debug.LogWarning("TrafficLightController: No traffic_light_* objects found under " + root.name, this);
    }

    private Transform ResolveSignalsRoot()
    {
        if (signalsRoot != null)
            return signalsRoot;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Equals("Traffic_light", System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        return transform;
    }

    private static bool IsTrafficLightInstance(string objectName)
    {
        return !string.IsNullOrEmpty(objectName) &&
               objectName.StartsWith("traffic_light_", System.StringComparison.OrdinalIgnoreCase);
    }

    private void AssignGroups()
    {
        groupA.Clear();
        groupB.Clear();

        for (int i = 0; i < signals.Count; i++)
        {
            TrafficLightSignal signal = signals[i];
            if (signal == null)
                continue;

            TrafficLightSignal.CrossingGroup group = signal.crossingGroup;
            if (group == TrafficLightSignal.CrossingGroup.Unassigned)
                group = ResolveGroupFromPosition(signal.transform.position);

            if (group == TrafficLightSignal.CrossingGroup.GroupB)
                groupB.Add(signal);
            else
                groupA.Add(signal);
        }
    }

    private TrafficLightSignal.CrossingGroup ResolveGroupFromPosition(Vector3 worldPosition)
    {
        Vector3 offset = worldPosition - intersectionCenter;
        float absX = Mathf.Abs(offset.x);
        float absZ = Mathf.Abs(offset.z);

        return absX > absZ
            ? TrafficLightSignal.CrossingGroup.GroupB
            : TrafficLightSignal.CrossingGroup.GroupA;
    }

    private void PrepareSignals()
    {
        for (int i = 0; i < signals.Count; i++)
        {
            TrafficLightSignal signal = signals[i];
            if (signal == null)
                continue;

            signal.ConfigureAudioSource();
            signal.ApplyClip(ringClip);
        }
    }

    private void BeginPhase(int groupIndex)
    {
        activeGroupIndex = groupIndex;
        phaseTimer = playbackDuration;
        StopAllSignals();
        PlayActiveGroup();
        ApplyTrafficState();
    }

    private void SwitchToNextPhase()
    {
        StopAllSignals();
        pauseFollowsGroupA = activeGroupIndex == 0;
        activeGroupIndex = activeGroupIndex == 0 ? 1 : 0;
        isPausedBetweenPhases = true;
        pauseTimer = switchPause;
        ApplyTrafficState();
    }

    private void ApplyTrafficState()
    {
        bool gameplayActive = CanRun();
        bool groupAActive = gameplayActive && !isPausedBetweenPhases && activeGroupIndex == 0;
        bool groupBActive = gameplayActive && !isPausedBetweenPhases && activeGroupIndex == 1;
        Bounds vehicleStopBounds = BuildStopBounds();
        Bounds pedestrianStopBounds = BuildPedestrianStopBounds();

        bool blockNorthSouth = gameplayActive &&
                               (groupAActive || (isPausedBetweenPhases && pauseFollowsGroupA));

        bool blockEastWest = gameplayActive &&
                             (groupBActive || (isPausedBetweenPhases && !pauseFollowsGroupA));

        if (northSouthSpawner != null)
            northSouthSpawner.SetCrossingBlocked(blockNorthSouth, vehicleStopBounds);

        if (eastWestSpawner != null)
            eastWestSpawner.SetCrossingBlocked(blockEastWest, vehicleStopBounds);

        bool blockNorthSouthPed = gameplayActive &&
                                  (isPausedBetweenPhases || !groupBActive);

        bool blockEastWestPed = gameplayActive &&
                                (isPausedBetweenPhases || !groupAActive);

        bool northSouthPedClosingSoon = gameplayActive &&
                                        groupBActive &&
                                        phaseTimer < pedestrianCrossingLeadTime;

        bool eastWestPedClosingSoon = gameplayActive &&
                                      groupAActive &&
                                      phaseTimer < pedestrianCrossingLeadTime;

        if (northSouthPedSpawner != null)
            northSouthPedSpawner.SetCrossingState(blockNorthSouthPed, northSouthPedClosingSoon, pedestrianStopBounds);

        if (eastWestPedSpawner != null)
            eastWestPedSpawner.SetCrossingState(blockEastWestPed, eastWestPedClosingSoon, pedestrianStopBounds);
    }

    private Bounds BuildStopBounds()
    {
        return BuildBoundsFromCorners(intersectionMin, intersectionMax);
    }

    private Bounds BuildPedestrianStopBounds()
    {
        if (!useSeparatePedestrianStopZone)
            return BuildStopBounds();

        return BuildBoundsFromCorners(pedestrianIntersectionMin, pedestrianIntersectionMax);
    }

    private static Bounds BuildBoundsFromCorners(Vector3 minCorner, Vector3 maxCorner)
    {
        Vector3 center = (minCorner + maxCorner) * 0.5f;
        Vector3 size = maxCorner - minCorner;
        size.y = Mathf.Max(size.y, 1f);
        return new Bounds(center, size);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.35f);
        DrawBoundsWire(BuildStopBounds());

        if (useSeparatePedestrianStopZone)
        {
            Gizmos.color = new Color(0.2f, 0.85f, 0.35f, 0.85f);
            DrawBoundsWire(BuildPedestrianStopBounds());
        }
    }

    private static void DrawBoundsWire(Bounds bounds)
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void PlayActiveGroup()
    {
        List<TrafficLightSignal> activeGroup = activeGroupIndex == 0 ? groupA : groupB;
        for (int i = 0; i < activeGroup.Count; i++)
            activeGroup[i].PlayLooping();
    }

    private void StopAllSignals()
    {
        for (int i = 0; i < signals.Count; i++)
        {
            if (signals[i] != null)
                signals[i].StopRing();
        }
    }

    private void OnDisable()
    {
        StopAllSignals();

        Bounds vehicleStopBounds = BuildStopBounds();
        Bounds pedestrianStopBounds = BuildPedestrianStopBounds();

        if (northSouthSpawner != null)
            northSouthSpawner.SetCrossingBlocked(false, vehicleStopBounds);

        if (eastWestSpawner != null)
            eastWestSpawner.SetCrossingBlocked(false, vehicleStopBounds);

        if (northSouthPedSpawner != null)
            northSouthPedSpawner.SetCrossingBlocked(false, pedestrianStopBounds);

        if (eastWestPedSpawner != null)
            eastWestPedSpawner.SetCrossingBlocked(false, pedestrianStopBounds);
    }
}

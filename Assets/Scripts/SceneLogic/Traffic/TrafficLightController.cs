using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alternates two groups of four pedestrian signals. Only the active group plays
/// the crossing cue clip, helping blind players know when it is safe to cross.
/// </summary>
public class TrafficLightController : MonoBehaviour
{
    [Header("Signals")]
    [Tooltip("Optional manual list. If empty, child objects named traffic_light* are discovered at runtime.")]
    public List<TrafficLightSignal> signals = new List<TrafficLightSignal>();

    [Tooltip("Center of the intersection used for auto-grouping.")]
    public Vector3 intersectionCenter = new Vector3(-2.2f, 0f, -0.6f);

    [Header("Intersection Stop Zone")]
    [Tooltip("World-space corner where cars stop before entering the crossing (x, y, z).")]
    public Vector3 intersectionMin = new Vector3(-19f, 0f, -19f);

    [Tooltip("World-space opposite corner of the crossing stop zone (x, y, z).")]
    public Vector3 intersectionMax = new Vector3(16f, 0f, 15f);

    [Header("Timing")]
    [Tooltip("How long the active group loops the clip before switching to the other crossing.")]
    public float playbackDuration = 18f;

    [Tooltip("Silence between the two crossing directions.")]
    public float switchPause = 0.8f;

    [Header("Audio")]
    public AudioClip ringClip;

    [Header("Traffic Link")]
    [Tooltip("North-south traffic stops while group A rings, and stays held until group B rings.")]
    public TrafficSpawner northSouthSpawner;

    [Tooltip("East-west traffic stops while group B rings, and stays held until group A rings.")]
    public TrafficSpawner eastWestSpawner;

    public bool autoFindSpawners = true;

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

    private void DiscoverSignals()
    {
        if (signals.Count > 0)
            return;

        TrafficLightSignal[] found = GetComponentsInChildren<TrafficLightSignal>(true);
        if (found.Length > 0)
        {
            signals.AddRange(found);
            return;
        }

        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("traffic_light"))
                continue;

            TrafficLightSignal signal = child.GetComponent<TrafficLightSignal>();
            if (signal == null)
                signal = child.gameObject.AddComponent<TrafficLightSignal>();

            signals.Add(signal);
        }
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
        Bounds stopBounds = BuildStopBounds();

        bool blockNorthSouth = gameplayActive &&
                               (groupAActive || (isPausedBetweenPhases && pauseFollowsGroupA));

        bool blockEastWest = gameplayActive &&
                             (groupBActive || (isPausedBetweenPhases && !pauseFollowsGroupA));

        if (northSouthSpawner != null)
            northSouthSpawner.SetCrossingBlocked(blockNorthSouth, stopBounds);

        if (eastWestSpawner != null)
            eastWestSpawner.SetCrossingBlocked(blockEastWest, stopBounds);
    }

    private Bounds BuildStopBounds()
    {
        Vector3 center = (intersectionMin + intersectionMax) * 0.5f;
        Vector3 size = intersectionMax - intersectionMin;
        size.y = Mathf.Max(size.y, 1f);
        return new Bounds(center, size);
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

        if (northSouthSpawner != null)
            northSouthSpawner.SetCrossingBlocked(false, BuildStopBounds());

        if (eastWestSpawner != null)
            eastWestSpawner.SetCrossingBlocked(false, BuildStopBounds());
    }
}

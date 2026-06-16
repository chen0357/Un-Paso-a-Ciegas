using System;
using UnityEngine;

[Serializable]
public class TrafficLaneConfig
{
    [Tooltip("Optional label shown in the Inspector.")]
    public string laneName = "Lane";

    public Transform spawnPoint;
    public Transform despawnPoint;

    [Header("Overrides (0 = use spawner default)")]
    public float spawnInterval;
    public float initialDelay;
    public float speed;
    public int maxActiveCars;
}

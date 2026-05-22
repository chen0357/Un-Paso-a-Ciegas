using UnityEngine;

/// <summary>
/// Marks colliders for the white cane (audio/haptics only). Does not affect collisionCount.
/// </summary>
public class SurfaceTag : MonoBehaviour
{
    [Header("Category")]
    public SurfaceType surfaceType = SurfaceType.Default;

    [Header("Optional Overrides")]
    [Tooltip("If set, plays this clip instead of the default clip for surfaceType on CaneAudioSystem.")]
    public AudioClip customHitClip;
}

public enum SurfaceType
{
    Default,
    Ground,
    Wall,
    Obstacle,
    Wood,
    Metal,
    TactilePaving
}
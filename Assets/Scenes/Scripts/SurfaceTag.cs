using UnityEngine;

public class SurfaceTag : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Default;
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
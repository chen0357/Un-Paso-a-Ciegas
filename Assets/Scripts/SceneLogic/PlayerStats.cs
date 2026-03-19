using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public Transform playerHead;
    public Transform playerRoot;

    public float totalDistanceMoved = 0f;

    private Vector3 lastPosition;

    private void Start()
    {
        if (playerRoot != null)
        {
            lastPosition = playerRoot.position;
        }
    }

    private void Update()
    {
        if (playerRoot == null) return;

        float distance = Vector3.Distance(lastPosition, playerRoot.position);
        totalDistanceMoved += distance;
        lastPosition = playerRoot.position;
    }
}
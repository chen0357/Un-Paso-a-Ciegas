using UnityEngine;

/// <summary>
/// Moves a car along a straight line and destroys it when it reaches the despawn point.
/// Attach to the car root; assign spawn and despawn transforms from TrafficSpawner.
/// </summary>
public class TrafficCar : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Runtime (set by spawner)")]
    public Transform despawnPoint;

    private Vector3 moveDirection;
    private float despawnDistance;

    public void Initialize(Transform spawn, Transform despawn, float moveSpeed)
    {
        speed = moveSpeed;
        despawnPoint = despawn;

        transform.position = spawn.position;
        transform.rotation = spawn.rotation;

        moveDirection = spawn.forward;
        despawnDistance = Vector3.Distance(spawn.position, despawn.position) + 2f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        transform.position += moveDirection * (speed * Time.deltaTime);

        if (despawnPoint != null &&
            Vector3.Distance(transform.position, despawnPoint.position) < 1.5f)
        {
            Destroy(gameObject);
            return;
        }

        if (despawnDistance > 0f)
        {
            despawnDistance -= speed * Time.deltaTime;
            if (despawnDistance <= 0f)
                Destroy(gameObject);
        }
    }
}

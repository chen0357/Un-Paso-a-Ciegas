using UnityEngine;

/// <summary>
/// Spawns traffic cars at a fixed interval. Place empty GameObjects at road start (spawn)
/// and road end (despawn), both facing the driving direction.
/// </summary>
public class TrafficSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject carPrefab;

    [Header("Path")]
    public Transform spawnPoint;
    public Transform despawnPoint;

    [Header("Timing")]
    public float spawnInterval = 4f;
    public float initialDelay = 1f;
    public float speed = 8f;
    public float speedVariation = 2f;

    [Header("Limits")]
    public int maxActiveCars = 6;

    private float timer;
    private int activeCars;

    private void Start()
    {
        timer = initialDelay;
    }

    private void Update()
    {
        if (carPrefab == null || spawnPoint == null || despawnPoint == null)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (activeCars >= maxActiveCars)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        timer = spawnInterval;
        SpawnCar();
    }

    private void SpawnCar()
    {
        GameObject car = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
        activeCars++;

        TrafficCar mover = car.GetComponent<TrafficCar>();
        if (mover == null)
            mover = car.AddComponent<TrafficCar>();

        float carSpeed = speed + Random.Range(-speedVariation, speedVariation);
        mover.Initialize(spawnPoint, despawnPoint, carSpeed);

        TrafficCarTracker tracker = car.GetComponent<TrafficCarTracker>();
        if (tracker == null)
            tracker = car.AddComponent<TrafficCarTracker>();
        tracker.spawner = this;
    }

    public void NotifyCarDestroyed()
    {
        activeCars = Mathf.Max(0, activeCars - 1);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null || despawnPoint == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPoint.position, 1f);
        Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(despawnPoint.position, 1f);
    }
}

/// <summary>
/// Notifies the spawner when a car is destroyed so the active count stays accurate.
/// </summary>
public class TrafficCarTracker : MonoBehaviour
{
    [HideInInspector] public TrafficSpawner spawner;

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyCarDestroyed();
    }
}

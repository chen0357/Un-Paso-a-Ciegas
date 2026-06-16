using UnityEngine;

public enum TrafficModelForwardAxis
{
    PositiveZ,
    PositiveX,
    NegativeZ,
    NegativeX
}

/// <summary>
/// Moves a car along a straight line and destroys it when it reaches the despawn point.
/// Attach to the car root; assign spawn and despawn transforms from TrafficSpawner.
/// </summary>
public class TrafficCar : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Model Alignment")]
    [Tooltip("Which local axis of the car model points to the front. Most imported FBX cars use PositiveX.")]
    public TrafficModelForwardAxis modelForwardAxis = TrafficModelForwardAxis.PositiveX;

    [Header("Runtime (set by spawner)")]
    public Transform despawnPoint;

    private Vector3 moveDirection;
    private float despawnDistance;
    private AudioSource engineAudio;

    private void Awake()
    {
        engineAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (engineAudio != null && !engineAudio.isPlaying)
            engineAudio.Play();
    }

    public void Initialize(Transform spawn, Transform despawn, float moveSpeed)
    {
        speed = moveSpeed;
        despawnPoint = despawn;

        transform.position = spawn.position;
        transform.rotation = GetAlignedRotation(spawn.rotation);

        moveDirection = spawn.forward;
        despawnDistance = Vector3.Distance(spawn.position, despawn.position) + 2f;
    }

    private Quaternion GetAlignedRotation(Quaternion spawnRotation)
    {
        Vector3 modelForward = AxisToVector(modelForwardAxis);
        Quaternion modelAlignment = Quaternion.FromToRotation(modelForward, Vector3.forward);
        return spawnRotation * modelAlignment;
    }

    private static Vector3 AxisToVector(TrafficModelForwardAxis axis)
    {
        switch (axis)
        {
            case TrafficModelForwardAxis.PositiveX:
                return Vector3.right;
            case TrafficModelForwardAxis.NegativeX:
                return Vector3.left;
            case TrafficModelForwardAxis.NegativeZ:
                return Vector3.back;
            default:
                return Vector3.forward;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
        {
            if (engineAudio != null && engineAudio.isPlaying)
                engineAudio.Pause();
            return;
        }

        if (engineAudio != null && !engineAudio.isPlaying)
            engineAudio.UnPause();

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

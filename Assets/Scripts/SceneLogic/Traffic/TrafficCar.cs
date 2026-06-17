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
    private const float StopLineTolerance = 0.05f;

    [Header("Movement")]
    public float speed = 8f;

    [Header("Following")]
    public float followDistance = 4.5f;
    public float slowdownDistance = 12f;
    public float minFollowSpeed = 1.5f;

    [Header("Model Alignment")]
    [Tooltip("Which local axis of the car model points to the front. Most imported FBX cars use PositiveX.")]
    public TrafficModelForwardAxis modelForwardAxis = TrafficModelForwardAxis.PositiveX;

    [Header("Runtime (set by spawner)")]
    public Transform despawnPoint;

    private Vector3 moveDirection;
    private float despawnDistance;
    private AudioSource engineAudio;
    private TrafficSpawner owningSpawner;
    private int laneIndex = -1;
    private bool heldAtStopLine;

    public int LaneIndex => laneIndex;
    public Vector3 MoveDirection => moveDirection;

    private void Awake()
    {
        engineAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (engineAudio != null && !engineAudio.isPlaying)
            engineAudio.Play();
    }

    public void Initialize(Transform spawn, Transform despawn, float moveSpeed, int lane)
    {
        laneIndex = lane;
        speed = moveSpeed;
        despawnPoint = despawn;

        transform.position = spawn.position;
        transform.rotation = GetAlignedRotation(spawn.rotation);

        moveDirection = spawn.forward.normalized;
        despawnDistance = Vector3.Distance(spawn.position, despawn.position) + 2f;
    }

    public void BindSpawner(TrafficSpawner spawner)
    {
        owningSpawner = spawner;
    }

    public float GetTravelAxis()
    {
        return Vector3.Dot(transform.position, moveDirection);
    }

    public float GetGapToCarAhead(TrafficCar ahead)
    {
        if (ahead == null)
            return float.MaxValue;

        return ahead.GetTravelAxis() - GetTravelAxis();
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

    private void UpdateEngineAudioState()
    {
        if (engineAudio == null)
            return;

        if (heldAtStopLine)
        {
            if (engineAudio.isPlaying)
                engineAudio.Pause();
            return;
        }

        if (!engineAudio.isPlaying)
            engineAudio.UnPause();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
        {
            if (engineAudio != null && engineAudio.isPlaying)
                engineAudio.Pause();
            return;
        }

        Vector3 currentPosition = transform.position;
        TrafficCar carAhead = owningSpawner != null
            ? owningSpawner.FindCarAhead(this, laneIndex)
            : null;

        float effectiveSpeed = ResolveFollowSpeed(carAhead);
        Vector3 desiredNext = currentPosition + moveDirection * (effectiveSpeed * Time.deltaTime);
        Vector3 nextPosition = ConstrainToCarAhead(currentPosition, desiredNext, carAhead);

        if (ShouldApplyIntersectionHold(owningSpawner))
            nextPosition = ApplyIntersectionStop(currentPosition, nextPosition, owningSpawner.StopBounds);

        heldAtStopLine = (nextPosition - currentPosition).sqrMagnitude <= 0.000001f &&
                         (effectiveSpeed <= 0.01f ||
                          ShouldApplyIntersectionHold(owningSpawner) ||
                          (carAhead != null && GetGapToCarAhead(carAhead) <= followDistance + 0.1f));

        UpdateEngineAudioState();

        Vector3 movement = nextPosition - currentPosition;
        if (movement.sqrMagnitude <= 0f)
            return;

        transform.position = nextPosition;

        if (despawnPoint != null &&
            Vector3.Distance(transform.position, despawnPoint.position) < 1.5f)
        {
            Destroy(gameObject);
            return;
        }

        if (despawnDistance > 0f)
        {
            despawnDistance -= movement.magnitude;
            if (despawnDistance <= 0f)
                Destroy(gameObject);
        }
    }

    private float ResolveFollowSpeed(TrafficCar carAhead)
    {
        if (carAhead == null)
            return speed;

        float gap = GetGapToCarAhead(carAhead);
        if (gap <= followDistance)
            return 0f;

        if (gap >= slowdownDistance)
            return speed;

        float t = Mathf.InverseLerp(followDistance, slowdownDistance, gap);
        return Mathf.Lerp(minFollowSpeed, speed, t);
    }

    private Vector3 ConstrainToCarAhead(Vector3 current, Vector3 next, TrafficCar carAhead)
    {
        if (carAhead == null)
            return next;

        float maxAxis = carAhead.GetTravelAxis() - followDistance;
        float nextAxis = Vector3.Dot(next, moveDirection);
        if (nextAxis <= maxAxis)
            return next;

        float currentAxis = Vector3.Dot(current, moveDirection);
        float clampedAxis = Mathf.Max(currentAxis, maxAxis);
        return current + moveDirection * (clampedAxis - currentAxis);
    }

    private bool ShouldApplyIntersectionHold(TrafficSpawner spawner)
    {
        if (spawner == null)
            return false;

        if (spawner.IsCrossingBlocked)
            return true;

        return spawner.IsReleaseHoldActive && !HasClearedIntersectionStop(spawner.StopBounds);
    }

    private bool HasClearedIntersectionStop(Bounds bounds)
    {
        Vector3 current = transform.position;

        if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
        {
            if (moveDirection.z > 0f)
                return current.z > bounds.max.z || current.z > bounds.min.z + StopLineTolerance;

            return current.z < bounds.min.z || current.z < bounds.max.z - StopLineTolerance;
        }

        if (moveDirection.x > 0f)
            return current.x > bounds.max.x || current.x > bounds.min.x + StopLineTolerance;

        return current.x < bounds.min.x || current.x < bounds.max.x - StopLineTolerance;
    }

    private Vector3 ApplyIntersectionStop(Vector3 current, Vector3 next, Bounds bounds)
    {
        if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
        {
            if (moveDirection.z > 0f)
            {
                if (current.z > bounds.max.z)
                    return next;

                if (current.z > bounds.min.z + StopLineTolerance)
                    return next;

                if (next.z > bounds.min.z)
                    return new Vector3(next.x, next.y, bounds.min.z);

                return next;
            }

            if (current.z < bounds.min.z)
                return next;

            if (current.z < bounds.max.z - StopLineTolerance)
                return next;

            if (next.z < bounds.max.z)
                return new Vector3(next.x, next.y, bounds.max.z);

            return next;
        }

        if (moveDirection.x > 0f)
        {
            if (current.x > bounds.max.x)
                return next;

            if (current.x > bounds.min.x + StopLineTolerance)
                return next;

            if (next.x > bounds.min.x)
                return new Vector3(bounds.min.x, next.y, next.z);

            return next;
        }

        if (current.x < bounds.min.x)
            return next;

        if (current.x < bounds.max.x - StopLineTolerance)
            return next;

        if (next.x < bounds.max.x)
            return new Vector3(bounds.max.x, next.y, next.z);

        return next;
    }
}

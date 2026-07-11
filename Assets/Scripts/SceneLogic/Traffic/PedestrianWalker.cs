using UnityEngine;

/// <summary>
/// Moves a pedestrian along a straight sidewalk path and destroys it at the despawn point.
/// Waits at the intersection until the linked traffic light allows crossing.
/// </summary>
public class PedestrianWalker : MonoBehaviour
{
    private const float StopLineTolerance = 0.05f;

    [Header("Movement")]
    public float speed = 1.8f;

    [Header("Following")]
    public float followDistance = 1.5f;
    public float slowdownDistance = 4f;
    public float minFollowSpeed = 0.4f;

    [Header("Placement")]
    public Vector3 spawnOffset = Vector3.zero;

    [Header("Runtime (set by spawner)")]
    public Transform despawnPoint;

    private Vector3 moveDirection;
    private float despawnDistance;
    private PedestrianSpawner owningSpawner;
    private int laneIndex = -1;

    public int LaneIndex => laneIndex;
    public Vector3 MoveDirection => moveDirection;

    public void Initialize(Transform spawn, Transform despawn, float moveSpeed, int lane, float lateralOffset = 0f)
    {
        laneIndex = lane;
        speed = moveSpeed;
        despawnPoint = despawn;

        Vector3 lateral = Vector3.Cross(Vector3.up, spawn.forward).normalized;
        transform.position = spawn.position + spawn.rotation * spawnOffset + lateral * lateralOffset;
        transform.rotation = spawn.rotation;

        moveDirection = spawn.forward.normalized;
        despawnDistance = Vector3.Distance(spawn.position, despawn.position) + 2f;
    }

    public void BindSpawner(PedestrianSpawner spawner)
    {
        owningSpawner = spawner;
    }

    public float GetTravelAxis()
    {
        return Vector3.Dot(transform.position, moveDirection);
    }

    public float GetGapToWalkerAhead(PedestrianWalker ahead)
    {
        if (ahead == null)
            return float.MaxValue;

        return ahead.GetTravelAxis() - GetTravelAxis();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        Vector3 currentPosition = transform.position;
        PedestrianWalker walkerAhead = owningSpawner != null
            ? owningSpawner.FindWalkerAhead(this, laneIndex)
            : null;

        float effectiveSpeed = ResolveFollowSpeed(walkerAhead);
        Vector3 desiredNext = currentPosition + moveDirection * (effectiveSpeed * Time.deltaTime);
        Vector3 nextPosition = ConstrainToWalkerAhead(currentPosition, desiredNext, walkerAhead);

        if (ShouldApplyIntersectionHold(owningSpawner))
            nextPosition = ApplyIntersectionStop(currentPosition, nextPosition, owningSpawner.StopBounds);

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

    private float ResolveFollowSpeed(PedestrianWalker walkerAhead)
    {
        if (walkerAhead == null)
            return speed;

        float gap = GetGapToWalkerAhead(walkerAhead);
        if (gap <= followDistance)
            return 0f;

        if (gap >= slowdownDistance)
            return speed;

        float t = Mathf.InverseLerp(followDistance, slowdownDistance, gap);
        return Mathf.Lerp(minFollowSpeed, speed, t);
    }

    private Vector3 ConstrainToWalkerAhead(Vector3 current, Vector3 next, PedestrianWalker walkerAhead)
    {
        if (walkerAhead == null)
            return next;

        float maxAxis = walkerAhead.GetTravelAxis() - followDistance;
        float nextAxis = Vector3.Dot(next, moveDirection);
        if (nextAxis <= maxAxis)
            return next;

        float currentAxis = Vector3.Dot(current, moveDirection);
        float clampedAxis = Mathf.Max(currentAxis, maxAxis);
        return current + moveDirection * (clampedAxis - currentAxis);
    }

    private bool ShouldApplyIntersectionHold(PedestrianSpawner spawner)
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

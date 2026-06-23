using UnityEngine;

public class SidewalkSafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerSidewalkTracker tracker = GetOrCreateTracker(other);
        if (tracker != null)
            tracker.EnterSidewalk();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerSidewalkTracker tracker = other.GetComponentInParent<PlayerSidewalkTracker>();
        if (tracker != null)
            tracker.ExitSidewalk();
    }

    private static PlayerSidewalkTracker GetOrCreateTracker(Collider other)
    {
        PlayerSidewalkTracker tracker = other.GetComponentInParent<PlayerSidewalkTracker>();
        if (tracker != null)
            return tracker;

        PlayerDamageReceiver receiver = other.GetComponentInParent<PlayerDamageReceiver>();
        if (receiver == null)
            return null;

        return receiver.gameObject.AddComponent<PlayerSidewalkTracker>();
    }
}

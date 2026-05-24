using UnityEngine;

public class DangerZone : MonoBehaviour
{
    public int damage = 10;
    public float damageInterval = 1f;

    private float timer;
    private PlayerDamageReceiver receiverInZone;

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        receiverInZone = receiver;
        timer = damageInterval;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        if (receiver != receiverInZone) return;

        receiverInZone = null;
        timer = 0f;
    }

    private void Update()
    {
        if (receiverInZone == null) return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        timer += Time.deltaTime;
        if (timer < damageInterval) return;

        timer = 0f;
        receiverInZone.ReceiveDamage(damage, gameObject.name);
    }
}

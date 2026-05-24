using UnityEngine;

public class DamageObject : MonoBehaviour
{
    public int damageAmount = 10;
    public float damageCooldown = 1f;

    private float lastDamageTime = -999f;
    private PlayerDamageReceiver currentReceiver;

    private void OnTriggerEnter(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        currentReceiver = receiver;

        if (GameManager.Instance != null && GameManager.Instance.CanProcessGameplay())
            GameManager.Instance.RegisterCollision();

        TryApplyDamage();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        currentReceiver = receiver;
        TryApplyDamage();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PlayerDamageReceiver.TryGetFromPlayerBodyCollider(other, out PlayerDamageReceiver receiver))
            return;

        if (receiver == currentReceiver)
        {
            currentReceiver = null;
        }
    }

    private void TryApplyDamage()
    {
        if (currentReceiver == null) return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;

        string source = transform.parent != null
            ? transform.parent.name + " / " + gameObject.name
            : gameObject.name;
        currentReceiver.ReceiveDamage(damageAmount, source);
    }
}
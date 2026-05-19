using UnityEngine;

public class DamageObject : MonoBehaviour
{
    public int damageAmount = 10;
    public float damageCooldown = 1f;

    private float lastDamageTime = -999f;
    private PlayerDamageReceiver currentReceiver;

    private void OnTriggerEnter(Collider other)
    {
        PlayerDamageReceiver receiver = other.GetComponentInParent<PlayerDamageReceiver>();

        if (receiver == null) return;

        currentReceiver = receiver;

        if (GameManager.Instance != null && GameManager.Instance.CanProcessGameplay())
            GameManager.Instance.RegisterCollision();

        TryApplyDamage();
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerDamageReceiver receiver = other.GetComponentInParent<PlayerDamageReceiver>();

        if (receiver == null) return;

        currentReceiver = receiver;
        TryApplyDamage();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerDamageReceiver receiver = other.GetComponentInParent<PlayerDamageReceiver>();

        if (receiver == null) return;

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

        currentReceiver.ReceiveDamage(damageAmount, gameObject.name);
    }
}
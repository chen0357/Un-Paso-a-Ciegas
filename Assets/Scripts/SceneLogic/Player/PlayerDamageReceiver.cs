using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    /// <summary>
    /// Only colliders on objects tagged "Player" count (e.g. PlayerTriggerDetector).
    /// Ignores cane tip and other child triggers that share the XR Origin hierarchy.
    /// </summary>
    public static bool TryGetFromPlayerBodyCollider(Collider other, out PlayerDamageReceiver receiver)
    {
        receiver = null;
        if (other == null || !other.CompareTag("Player"))
            return false;

        receiver = other.GetComponentInParent<PlayerDamageReceiver>();
        return receiver != null;
    }

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerDamageReceiver 找不到 PlayerHealth，请确认它们都挂在 XR Origin 根物体上");
        }
        else
        {
            Debug.Log("PlayerDamageReceiver 初始化成功");
        }
    }

    public void ReceiveDamage(int damage, string damageSource = "")
    {
        Debug.Log("ReceiveDamage 被调用，伤害值: " + damage + " 来源: " + damageSource);

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (playerHealth == null)
        {
            Debug.LogError("无法扣血，因为 PlayerHealth 是 null");
            return;
        }

        playerHealth.TakeDamage(damage);

        if (!string.IsNullOrEmpty(damageSource))
        {
            Debug.Log("伤害来源: " + damageSource);
        }
    }
}
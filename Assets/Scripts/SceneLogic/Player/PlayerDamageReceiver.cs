using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
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

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            Debug.Log("游戏已经结束，不再扣血");
            return;
        }

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
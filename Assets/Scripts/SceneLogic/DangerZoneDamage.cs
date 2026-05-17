using UnityEngine;

public class DangerZone : MonoBehaviour
{
    public int damage = 10;
    public float damageInterval = 1f;

    private float timer = 0f;
    private PlayerHealth playerInZone;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            playerInZone = player;
            timer = damageInterval;
            Debug.Log("Player entered danger zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null && player == playerInZone)
        {
            playerInZone = null;
            timer = 0f;
            Debug.Log("Player exited danger zone");
        }
    }

    private void Update()
    {
        if (playerInZone == null) return;

        timer += Time.deltaTime;

        if (timer >= damageInterval)
        {
            playerInZone.TakeDamage(damage);
            timer = 0f;
        }
    }
}
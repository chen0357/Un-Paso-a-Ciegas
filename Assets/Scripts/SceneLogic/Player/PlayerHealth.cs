using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        Debug.Log("PlayerHealth inicializado correctamente. Salud actual: " + currentHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogError("PlayerHealth no encontro GameManager.Instance");
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("TakeDamage llamado. Dano: " + damage);

        if (isDead) return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            Debug.Log("El juego ya termino. TakeDamage detenido.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Dano aplicado correctamente. Salud actual: " + currentHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            isDead = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.FailGame("salud agotada");
            }
        }
    }
}
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteGame();
            }
        }
    }
}
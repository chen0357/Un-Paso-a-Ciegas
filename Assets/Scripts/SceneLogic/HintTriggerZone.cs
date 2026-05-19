using UnityEngine;

public class HintTriggerZone : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (GameManager.Instance != null && !GameManager.Instance.CanProcessGameplay())
            return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterHint();
            }

            Debug.Log("¥•∑¢Ã· æ");
        }
    }
}
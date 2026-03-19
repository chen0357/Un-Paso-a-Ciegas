using UnityEngine;

public class HintTriggerZone : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("¥•∑¢Ã· æ");
            GameManager.Instance.RegisterHint();
        }
    }
}
using UnityEngine;

public class DangerZoneTrigger : MonoBehaviour
{
    public string failReason = "½øÈëÎ£ÏÕÇøÓò";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.FailGame(failReason);
        }
    }
}
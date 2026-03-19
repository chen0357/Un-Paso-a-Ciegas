using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public UIManager uiManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiManager != null)
            {
                uiManager.ShowResult();
            }

            Time.timeScale = 0f;
            Debug.Log("µΩ¥Ô÷’µ„");
        }
    }
}
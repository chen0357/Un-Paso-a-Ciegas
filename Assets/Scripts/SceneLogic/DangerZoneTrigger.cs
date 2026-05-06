using UnityEngine;

public class DangerZoneTrigger : MonoBehaviour
{
    public UIManager uiManager;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("触发到了: " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("进入危险区域");

            if (uiManager != null)
            {
                uiManager.ShowResult(); // 关键
            }

            Time.timeScale = 0f; // 暂停游戏
        }
    }
}
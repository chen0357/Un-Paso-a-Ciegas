using UnityEngine;

public class PlayerTriggerDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("触发: " + other.name);

        if (other.CompareTag("Danger"))
        {
            Debug.Log("进入危险区域");

            UIManager ui = FindObjectOfType<UIManager>();
            if (ui != null)
            {
                ui.ShowResult();
            }

            Time.timeScale = 0f;
        }

        if (other.CompareTag("Goal"))
        {
            Debug.Log("到达终点");

            UIManager ui = FindObjectOfType<UIManager>();
            if (ui != null)
            {
                ui.ShowResult();
            }

            Time.timeScale = 0f;
        }

        if (other.CompareTag("Hint"))
        {
            Debug.Log("触发提示");
        }
    }
}
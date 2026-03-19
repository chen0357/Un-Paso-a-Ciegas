using TMPro;
using UnityEngine;

public class ResultUIController : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject resultPanel;
    public TMP_Text titleText;
    public TMP_Text timeText;
    public TMP_Text collisionText;
    public TMP_Text hintText;
    public TMP_Text reasonText;

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    public void ShowResult(bool success, float finishTime, int collisionCount, int hintCount, string reason = "")
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = success ? "体验完成" : "体验失败";
        }

        if (timeText != null)
        {
            timeText.text = "完成时间: " + finishTime.ToString("F1") + " 秒";
        }

        if (collisionText != null)
        {
            collisionText.text = "碰撞次数: " + collisionCount;
        }

        if (hintText != null)
        {
            hintText.text = "提示触发次数: " + hintCount;
        }

        if (reasonText != null)
        {
            reasonText.text = success ? "你已成功到达目标区域" : "失败原因: " + reason;
        }

        Time.timeScale = 0f;
    }
}
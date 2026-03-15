//using TMPro;
//using UnityEngine;

//public class ResultManager : MonoBehaviour
//{
//    public TMP_Text timeText;
//    public TMP_Text hitText;
//    public TMP_Text hintText;

//    private float finishTime;
//    private int hitCount;
//    private int hintCount;

//    public void SetResults(float time, int hits, int hints)
//    {
//        finishTime = time;
//        hitCount = hits;
//        hintCount = hints;

//        UpdateUI();
//    }

//    public void UpdateUI()
//    {
//        if (timeText != null)
//            timeText.text = "完成时间: " + finishTime.ToString("F1") + " 秒";

//        if (hitText != null)
//            hitText.text = "碰撞次数: " + hitCount;

//        if (hintText != null)
//            hintText.text = "提示触发次数: " + hintCount;
//    }
//}

using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public GameObject resultPanel;

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    public void ShowResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    public void HideResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
}
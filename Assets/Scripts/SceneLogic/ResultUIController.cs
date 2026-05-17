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

    private bool resultShown = false;

    private void Start()
    {
        resultShown = false;

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    public void ShowResult(bool success, float finishTime, int collisionCount, int hintCount, string reason = "")
    {
        if (resultShown) return;
        resultShown = true;

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = success ? "Experiencia completada" : "Experiencia fallida";
        }

        if (timeText != null)
        {
            timeText.text = "Tiempo completado: " + finishTime.ToString("F1") + " s";
        }

        if (collisionText != null)
        {
            collisionText.text = "N¨²mero de colisiones: " + collisionCount;
        }

        if (hintText != null)
        {
            hintText.text = "N¨²mero de pistas activadas: " + hintCount;
        }

        if (reasonText != null)
        {
            reasonText.text = success
                ? "Has llegado exitosamente al ¨¢rea objetivo"
                : "Raz¨®n del fallo: " + reason;
        }

        Time.timeScale = 0f;
    }
}
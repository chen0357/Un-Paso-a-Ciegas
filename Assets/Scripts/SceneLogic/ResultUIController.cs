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

    private void Awake()
    {
        HideResultPanel();
    }

    private void Start()
    {
        HideResultPanel();
    }

    private void HideResultPanel()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void ShowResult(bool success, float finishTime, int collisionCount, int hintCount, string reason = "")
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (titleText != null)
            titleText.text = success ? "Experiencia completada" : "Experiencia fallida";

        if (timeText != null)
            timeText.text = "Tiempo completado: " + finishTime.ToString("F1") + " s";

        if (collisionText != null)
            collisionText.text = "Numero de colisiones: " + collisionCount;

        if (hintText != null)
            hintText.text = "Numero de pistas activadas: " + hintCount;

        if (reasonText != null)
        {
            reasonText.text = success
                ? "Has llegado exitosamente al area objetivo"
                : "Razon del fallo: " + reason;
        }

        Time.timeScale = 0f;
    }
}

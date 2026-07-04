using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameIntroPanelController : MonoBehaviour
{
    private static readonly string[] PageTitles =
    {
        "Objetivo de la experiencia",
        "Contexto y reflexion",
        "Como jugar"
    };

    private static readonly string[] PageContents =
    {
        "VLVS: Un Paso a Ciegas es un prototipo de realidad virtual creado para reflexionar sobre la accesibilidad urbana desde la perspectiva de una persona con discapacidad visual.\n\nLa experiencia no se limita a simular una alteracion visual: su objetivo es ayudarte a comprender como cambian la orientacion, la seguridad y la autonomia cuando la informacion visual es reducida o inexistente.",
        "A lo largo del recorrido encontraras situaciones inspiradas en la vida cotidiana, como desplazarte por la ciudad, localizar referencias del entorno o cruzar una calle con seguridad.\n\nEl proyecto pone el foco en las barreras que aparecen cuando faltan ayudas accesibles, y propone una reflexion sobre como el diseno urbano, las senales sonoras y los elementos tactiles pueden influir en la movilidad de las personas con discapacidad visual.",
        "1. Elige un nivel y, antes de empezar, selecciona un modo visual que simule un grado de discapacidad visual.\n2. Avanza despacio y explora el entorno con el baston para reconocer rutas, obstaculos y referencias cercanas.\n3. Usa las pistas de audio espacial y las senales del entorno para orientarte, localizar objetivos y detectar zonas de peligro.\n4. Completa la tarea propuesta, como cruzar la calle o llegar al destino, evitando choques y situaciones de riesgo.\n\nConsejo: durante el recorrido, observa como el diseno del entorno influye en tu seguridad y en tu autonomia."
    };

    private TMP_Text titleText;
    private TMP_Text contentText;
    private TMP_Text pageIndicatorText;
    private Button previousButton;
    private Button nextButton;
    private int currentPage;

    private void Awake()
    {
        CacheReferences();
        BindButtons();
    }

    private void OnEnable()
    {
        CacheReferences();
        BindButtons();
        SetPage(0);
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    public void ShowPreviousPage()
    {
        SetPage(currentPage - 1);
    }

    public void ShowNextPage()
    {
        SetPage(currentPage + 1);
    }

    private void SetPage(int pageIndex)
    {
        currentPage = Mathf.Clamp(pageIndex, 0, PageTitles.Length - 1);
        RefreshPage();
    }

    private void RefreshPage()
    {
        if (titleText != null)
            titleText.text = PageTitles[currentPage];

        if (contentText != null)
            contentText.text = PageContents[currentPage];

        if (pageIndicatorText != null)
            pageIndicatorText.text = $"{currentPage + 1} / {PageTitles.Length}";

        if (previousButton != null)
            previousButton.interactable = currentPage > 0;

        if (nextButton != null)
            nextButton.interactable = currentPage < PageTitles.Length - 1;
    }

    private void CacheReferences()
    {
        if (titleText == null)
        {
            Transform title = transform.Find("Title");
            if (title != null)
                titleText = title.GetComponent<TMP_Text>();
        }

        if (contentText == null)
        {
            Transform content = transform.Find("ContentPanel/ContentText");
            if (content != null)
                contentText = content.GetComponent<TMP_Text>();
        }

        if (pageIndicatorText == null)
        {
            Transform indicator = transform.Find("PageIndicator");
            if (indicator != null)
                pageIndicatorText = indicator.GetComponent<TMP_Text>();
        }

        if (previousButton == null)
        {
            Transform prevButton = transform.Find("Btn_Prev");
            if (prevButton != null)
                previousButton = prevButton.GetComponent<Button>();
        }

        if (nextButton == null)
        {
            Transform nextButtonTransform = transform.Find("Btn_Next");
            if (nextButtonTransform != null)
                nextButton = nextButtonTransform.GetComponent<Button>();
        }
    }

    private void BindButtons()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(ShowPreviousPage);
            previousButton.onClick.AddListener(ShowPreviousPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(ShowNextPage);
            nextButton.onClick.AddListener(ShowNextPage);
        }
    }

    private void UnbindButtons()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(ShowPreviousPage);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(ShowNextPage);
    }
}

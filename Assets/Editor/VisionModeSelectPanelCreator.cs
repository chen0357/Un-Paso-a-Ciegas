#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class VisionModeSelectPanelCreator
{
    private const string SourcePath = "Assets/UI/Prefabs/LevelSelectPanel.prefab";
    private const string TargetPath = "Assets/UI/Prefabs/VisionModeSelectPanel.prefab";

    [MenuItem("Tools/UI/Create Vision Mode Select Panel")]
    public static void CreatePanel()
    {
        if (!File.Exists(SourcePath))
        {
            Debug.LogError($"Source prefab not found: {SourcePath}");
            return;
        }

        if (AssetDatabase.CopyAsset(SourcePath, TargetPath))
        {
            Debug.Log($"Created {TargetPath}. Run 'Tools/UI/Configure Vision Mode Select Panel' next.");
        }
        else if (File.Exists(TargetPath))
        {
            Debug.LogWarning($"{TargetPath} already exists. Run Configure instead.");
        }
    }

    [MenuItem("Tools/UI/Configure Vision Mode Select Panel")]
    public static void ConfigurePanel()
    {
        var root = PrefabUtility.LoadPrefabContents(TargetPath);
        if (root == null)
        {
            Debug.LogError($"Could not load {TargetPath}");
            return;
        }

        try
        {
            root.name = "VisionModeSelectPanel";

            var controller = root.GetComponent<VisionModeSelectController>();
            if (controller == null)
                controller = root.AddComponent<VisionModeSelectController>();

            SetTitle(root, "Seleccionar modo visual");

            ConfigureVisionButton(FindChild(root, "Button_Experiencia al cruzar la calle"), "Button_Modo para ciegos", nameof(VisionModeSelectController.ChooseNearlyBlind));
            ConfigureVisionButton(FindChild(root, "Button_Experiencia en la parada de autobús"), "Button_Modo para baja visión", nameof(VisionModeSelectController.ChooseBlurry));
            ConfigureVisionButton(FindChild(root, "Button_Experiencia en interiores"), "Button_Modo discapacidad visual", nameof(VisionModeSelectController.ChooseVisualDisability));

            ConfigureBackButton(FindChild(root, "Button"), controller);

            PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
            Debug.Log($"Configured {TargetPath}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform FindChild(GameObject root, string childName)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == childName)
                return transform;
        }

        Debug.LogWarning($"Child not found: {childName}");
        return null;
    }

    private static void SetTitle(GameObject root, string title)
    {
        var titleTransform = FindChild(root, "Title");
        if (titleTransform == null)
            return;

        var text = titleTransform.GetComponent<TMP_Text>();
        if (text != null)
            text.text = title;
    }

    private static void ConfigureVisionButton(Transform buttonTransform, string label, string methodName)
    {
        if (buttonTransform == null)
            return;

        buttonTransform.gameObject.name = label;

        var text = buttonTransform.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = label;

        var button = buttonTransform.GetComponent<Button>();
        if (button == null)
            return;

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() =>
        {
            var controller = buttonTransform.GetComponentInParent<VisionModeSelectController>();
            if (controller == null)
                return;

            switch (methodName)
            {
                case nameof(VisionModeSelectController.ChooseNearlyBlind):
                    controller.ChooseNearlyBlind();
                    break;
                case nameof(VisionModeSelectController.ChooseBlurry):
                    controller.ChooseBlurry();
                    break;
                case nameof(VisionModeSelectController.ChooseVisualDisability):
                    controller.ChooseVisualDisability();
                    break;
            }
        });
    }

    private static void ConfigureBackButton(Transform buttonTransform, VisionModeSelectController controller)
    {
        if (buttonTransform == null || controller == null)
            return;

        var text = buttonTransform.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = "Volver";

        var button = buttonTransform.GetComponent<Button>();
        if (button == null)
            return;

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(controller.BackToLevelSelect);
    }
}
#endif

using UnityEngine;

public class VisionModeSelectController : MonoBehaviour
{
    private string pendingSceneName;

    public void BeginSelection(string sceneName)
    {
        pendingSceneName = sceneName;
    }

    public void ChooseNearlyBlind()
    {
        ConfirmVisionMode(0);
    }

    public void ChooseBlurry()
    {
        ConfirmVisionMode(1);
    }

    public void ChooseVisualDisability()
    {
        ConfirmVisionMode(2);
    }

    public void BackToLevelSelect()
    {
        pendingSceneName = null;

        var ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
            ui.BackFromVisionModeSelect();
    }

    private void ConfirmVisionMode(int mode)
    {
        if (string.IsNullOrEmpty(pendingSceneName))
            return;

        if (VisionModeManager.Instance != null)
            VisionModeManager.Instance.SetVisionMode((VisionModeManager.VisionMode)mode);
        else
        {
            PlayerPrefs.SetInt("VisionMode", mode);
            PlayerPrefs.Save();
        }

        string scene = pendingSceneName;
        pendingSceneName = null;

        var ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
            ui.LoadLevel(scene);
    }
}

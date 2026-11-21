using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private LoadingController loadingController;
    [SerializeField] private Button continueButton;
    [SerializeField] private SaveSlotPanel saveSlotPanel;

    private bool initialized;

    private bool HasRequiredReferences =>
        loadingController != null && continueButton != null && saveSlotPanel != null;

    private void Awake()
    {
        if (!HasRequiredReferences)
        {
            Debug.LogError("MainMenuController 缺少必要參考。", this);
            enabled = false;
            return;
        }

        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        saveSlotPanel.SlotsChanged += RefreshContinueButton;
        RefreshContinueButton();
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        saveSlotPanel.SlotsChanged -= RefreshContinueButton;
    }

    private void RefreshContinueButton()
    {
        continueButton.interactable = SaveSlots.HasLoadableSave();
    }

    public void StartNewGame()
    {
        GameLaunchRequest.RequestNewGame();
        loadingController.LoadScene(SceneNames.Main, "建立新遊戲...");
    }

    public void ContinueGame()
    {
        saveSlotPanel.OpenLoad();
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Quit game requested.");
#endif
    }
}
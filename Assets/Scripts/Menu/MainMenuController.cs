using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private LoadingController loadingController;
    [SerializeField] private Button continueButton;

    [SerializeField] private SaveSlotPanel saveSlotPanel;
    private void Awake()
    {
        if (loadingController == null)
        {
            loadingController =
                FindFirstObjectByType<LoadingController>();
        }
    }
    private void OnEnable()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SlotsChanged +=
                RefreshContinueButton;
        }

        RefreshContinueButton();
    }

    private void OnDisable()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SlotsChanged -=
                RefreshContinueButton;
        }
    }

    private void RefreshContinueButton()
    {
        if (continueButton == null)
        {
            return;
        }

        continueButton.interactable =
            SaveSlots.HasLoadableSave();
    }

    public void StartNewGame()
    {
        if (loadingController == null)
        {
            Debug.LogError(
                "MainMenuController 找不到 LoadingController。",
                this
            );

            return;
        }

        GameLaunchRequest.RequestNewGame();

        loadingController.LoadScene(
            SceneNames.Main,
            "建立新遊戲..."
        );
    }

    public void ContinueGame()
    {
        if (saveSlotPanel == null)
        {
            Debug.LogError(
                "MainMenu 尚未設定 SaveSlotPanel。",
                this
            );

            return;
        }

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
using UnityEngine;

public sealed class GameSystemPanel : MonoBehaviour
{

    [SerializeField] private GameSaveSystem gameSaveSystem;
    [SerializeField] private PanelController panelController;
    [SerializeField] private SaveSlotPanel saveSlotPanel;
    [SerializeField] private LoadingController loadingController;

    private void Awake()
    {
        if (gameSaveSystem == null)
            gameSaveSystem = FindFirstObjectByType<GameSaveSystem>();
        if (panelController == null)
            panelController = FindFirstObjectByType<PanelController>();
        if (saveSlotPanel == null)
            saveSlotPanel = FindFirstObjectByType<SaveSlotPanel>();
        if (loadingController == null)
            loadingController = FindFirstObjectByType<LoadingController>();
    }

    public void ContinueGame()
    {
        if (panelController != null)
            panelController.CloseCurrentPanel();
    }

    public void SaveGame()
    {
        if (saveSlotPanel == null)
        {
            Debug.LogError(
                "GameSystemPanel 找不到 SaveSlotPanel。",
                this
            );

            return;
        }

        if (panelController != null)
        {
            panelController.CloseCurrentPanel();
        }

        saveSlotPanel.OpenSave();
    }

    public void LoadGame()
    {
        if (saveSlotPanel == null)
        {
            Debug.LogError(
                "GameSystemPanel 找不到 SaveSlotPanel。",
                this
            );

            return;
        }

        if (panelController != null)
        {
            panelController.CloseCurrentPanel();
        }

        saveSlotPanel.OpenLoad();
    }

    public void ReturnToGameMenu()
    {
        if (loadingController == null)
        {
            Debug.LogError(
                "GameSystemPanel 找不到 LoadingController。",
                this
            );

            return;
        }

        if (panelController != null)
        {
            panelController.CloseCurrentPanel();
        }

        loadingController.LoadScene(
            SceneNames.MainMenu,
            "返回主選單..."
        );
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSystemPanel : MonoBehaviour
{
    [SerializeField] private GameSaveSystem gameSaveSystem;
    [SerializeField] private PanelController panelController;
    [SerializeField] private SaveSlotPanel saveSlotPanel;

    private void Awake()
    {
        if (gameSaveSystem == null)
            gameSaveSystem = FindFirstObjectByType<GameSaveSystem>();

        if (panelController == null)
            panelController =
                FindFirstObjectByType<PanelController>();
        if (saveSlotPanel == null)
        {
            saveSlotPanel =
                FindFirstObjectByType<SaveSlotPanel>();
        }
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
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
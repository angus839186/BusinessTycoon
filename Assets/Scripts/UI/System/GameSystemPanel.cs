using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSystemPanel : MonoBehaviour
{
    [SerializeField] private GameSaveSystem gameSaveSystem;
    [SerializeField] private ManagementPanelController panelController;

    private void Awake()
    {
        if (gameSaveSystem == null)
            gameSaveSystem = FindFirstObjectByType<GameSaveSystem>();

        if (panelController == null)
            panelController =
                FindFirstObjectByType<ManagementPanelController>();
    }

    public void ContinueGame()
    {
        if (panelController != null)
            panelController.CloseCurrentPanel();
    }

    public void SaveGame()
    {
        if (gameSaveSystem == null)
        {
            Debug.LogError("GameSaveSystem missing.");
            return;
        }

        gameSaveSystem.SaveGame();
    }

    public void ReturnToGameMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
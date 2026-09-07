using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void Start()
    {
        if (continueButton != null)
            continueButton.interactable =
                GameSaveSystem.DefaultSaveExists;
    }

    public void StartNewGame()
    {
        GameLaunchRequest.RequestNewGame();
        SceneManager.LoadScene(SceneNames.Main);
    }

    public void ContinueGame()
    {
        if (!GameSaveSystem.DefaultSaveExists)
        {
            Debug.LogWarning("Save file not found.");
            return;
        }

        GameLaunchRequest.RequestContinueGame();
        SceneManager.LoadScene(SceneNames.Main);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Quit game requested.");
#endif
    }
}
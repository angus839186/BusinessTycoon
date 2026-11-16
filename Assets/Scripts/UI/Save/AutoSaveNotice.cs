using UnityEngine;

public sealed class AutoSaveNotice : MonoBehaviour
{
    [SerializeField]
    private AutoSaveSystem autoSaveSystem;

    [SerializeField]
    private NoticeUI noticeUI;

    private void Awake()
    {
        if (autoSaveSystem == null)
        {
            autoSaveSystem =
                GetComponent<AutoSaveSystem>();
        }

        if (noticeUI == null)
        {
            noticeUI =
                FindFirstObjectByType<NoticeUI>();
        }
    }

    private void OnEnable()
    {
        if (autoSaveSystem == null)
            return;

        autoSaveSystem.AutoSaved +=
            HandleAutoSaved;

        autoSaveSystem.AutoSaveFailed +=
            HandleAutoSaveFailed;
    }

    private void OnDisable()
    {
        if (autoSaveSystem == null)
            return;

        autoSaveSystem.AutoSaved -=
            HandleAutoSaved;

        autoSaveSystem.AutoSaveFailed -=
            HandleAutoSaveFailed;
    }

    private void HandleAutoSaved()
    {
        if (noticeUI == null)
            return;

        noticeUI.ShowSuccess(
            "遊戲已自動儲存"
        );
    }

    private void HandleAutoSaveFailed(
        string errorMessage
    )
    {
        if (noticeUI == null)
            return;

        noticeUI.ShowError(
            "自動存檔失敗，請稍後再試"
        );
    }
}
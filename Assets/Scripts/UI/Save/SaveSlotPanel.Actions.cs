using System;
using UnityEngine;

public sealed partial class SaveSlotPanel
{
    private void ShowSuccess(string message)
    {
        SetFeedback(message);

        if (noticeUI != null)
        {
            noticeUI.ShowSuccess(message);
        }
    }

    private void ShowError(string message)
    {
        SetFeedback(message);

        if (noticeUI != null)
        {
            noticeUI.ShowError(message);
        }
    }

    private void HandleDeleteRequested(
    string slotId
)
    {
        if (confirmDialog == null)
        {
            const string message =
                "無法開啟刪除確認視窗。";

            Debug.LogError(message, this);
            ShowError(message);
            return;
        }

        confirmDialog.Show(
            "刪除存檔",
            "確定要刪除這個存檔嗎？\n" +
            "刪除後無法復原。",
            () => DeleteSlot(slotId)
        );
    }

    private void DeleteSlot(string slotId)
    {
        if (!SaveSlots.TryDelete(
                slotId,
                out string errorMessage
            ))
        {
            string message =
                $"刪除失敗：{errorMessage}";

            Debug.LogError(message, this);
            ShowError(message);
            return;
        }

        ShowSuccess("存檔已刪除。");

        Refresh();
        SlotsChanged?.Invoke();
    }
    private void HandleSlotAction(string slotId)
    {
        if (mode == SaveSlotMode.Save)
        {
            SaveToSlot(slotId);
            return;
        }

        LoadFromSlot(slotId);
    }

    private void SaveToSlot(string slotId)
    {
        if (gameSaveSystem == null)
        {
            const string message =
                "找不到遊戲存檔系統。";

            Debug.LogError(message, this);
            ShowError(message);
            return;
        }

        if (!gameSaveSystem.SetActiveSlot(slotId))
        {
            string message =
                $"無法選擇存檔槽：{slotId}";

            Debug.LogError(message, this);
            ShowError(message);
            return;
        }

        try
        {
            gameSaveSystem.SaveGame();

            ShowSuccess("遊戲已儲存。");

            Refresh();
            SlotsChanged?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);

            ShowError(
                "儲存失敗，請稍後再試。"
            );
        }
    }

    private void LoadFromSlot(string slotId)
    {
        if (gameSaveSystem != null)
        {
            if (loadingController == null)
            {
                ShowError("找不到載入畫面控制器。");
                return;
            }

            if (!gameSaveSystem.SetActiveSlot(slotId))
            {
                ShowError(
                    $"無法選擇存檔槽：{slotId}"
                );

                return;
            }

            loadingController.RunTask(
                "讀取遊戲...",
                gameSaveSystem.TryLoadGame,
                HandleLoadCompleted
            );

            return;
        }

        if (loadingController == null)
        {
            ShowError(
                "找不到載入畫面控制器。"
            );

            return;
        }

        if (!GameLaunchRequest.RequestContinueGame(
                slotId
            ))
        {
            ShowError(
                "無法讀取選擇的存檔。"
            );

            return;
        }

        loadingController.LoadScene(
            SceneNames.Main,
            "讀取遊戲..."
        );
    }

    private void HandleLoadCompleted(bool succeeded)
    {
        if (!succeeded)
        {
            ShowError(
                "讀取失敗，請確認存檔是否有效。"
            );

            return;
        }

        ShowSuccess("遊戲已讀取。");
        Close();
    }
}
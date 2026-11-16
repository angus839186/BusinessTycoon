using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public enum SaveSlotMode
{
    Load,
    Save
}

public sealed class SaveSlotPanel : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameSaveSystem gameSaveSystem;
    
    [Header("UI")]
    [SerializeField] private NoticeUI noticeUI;
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform listRoot;
    [SerializeField] private SaveSlotItem itemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Dialog")]
    [SerializeField] private ConfirmDialog confirmDialog;

    public event Action SlotsChanged;
    private ReusableUIList<SaveSlotItem> itemList;
    private SaveSlotMode mode;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput =
                FindFirstObjectByType<GameInput>();
        }
        if (gameSaveSystem == null)
        {
            gameSaveSystem =
                FindFirstObjectByType<GameSaveSystem>();
        }

        if (
            itemPrefab != null &&
            listRoot != null
        )
        {
            itemList =
                new ReusableUIList<SaveSlotItem>(
                    itemPrefab,
                    listRoot
                );
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }
        if (noticeUI == null)
        {
            noticeUI =
                FindFirstObjectByType<NoticeUI>();
        }
    }
    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed +=
                HandleCancelPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed -=
                HandleCancelPressed;
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    public void OpenLoad()
    {
        mode = SaveSlotMode.Load;
        Open();
    }

    public void OpenSave()
    {
        mode = SaveSlotMode.Save;
        Open();
    }

    public void Close()
    {
        if (confirmDialog != null && confirmDialog.IsOpen)
        {
            confirmDialog.Hide();
        }
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }

        if (gameInput != null)
        {
            gameInput.SetMapEnabled(true);
        }
    }

    private void Open()
    {
        if (windowRoot == null)
        {
            Debug.LogError(
                "SaveSlotPanel 尚未設定 Window Root。",
                this
            );

            return;
        }
        SetFeedback(string.Empty);
        windowRoot.SetActive(true);
        if (gameInput != null)
        {
            gameInput.SetMapEnabled(false);
        }
        Refresh();
    }

    private void Refresh()
    {
        if (itemList == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text =
                mode == SaveSlotMode.Load
                    ? "讀取遊戲"
                    : "儲存遊戲";
        }

        IReadOnlyList<SaveSlotInfo> infos =
    mode == SaveSlotMode.Load
        ? SaveSlots.GetLoadInfos()
        : SaveSlots.GetInfos();

        int index = 0;

        foreach (SaveSlotInfo info in infos)
        {
            SaveSlotItem item =
                itemList.GetOrCreate(index);

            if (item == null)
            {
                continue;
            }

            bool actionEnabled =
                CanUseSlot(info);

            bool deleteEnabled = info != null && info.Exists;

            item.Bind(info, mode == SaveSlotMode.Load ? "讀取" : "儲存",
            actionEnabled, HandleSlotAction, deleteEnabled, HandleDeleteRequested);

            index++;
        }

        itemList.HideFrom(index);
    }
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

    private void SetFeedback(string message)
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.text = message;

        feedbackText.gameObject.SetActive(
            !string.IsNullOrWhiteSpace(message)
        );
    }

    private bool CanUseSlot(SaveSlotInfo info)
    {
        if (info == null)
        {
            return false;
        }

        if (mode == SaveSlotMode.Save)
        {
            return gameSaveSystem != null;
        }

        return
            info.Exists &&
            info.CanLoad &&
            string.IsNullOrWhiteSpace(
                info.ErrorMessage
            );
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
            if (!gameSaveSystem.SetActiveSlot(slotId))
            {
                ShowError(
                    $"無法選擇存檔槽：{slotId}"
                );

                return;
            }

            if (!gameSaveSystem.TryLoadGame())
            {
                ShowError(
                    "讀取失敗，請確認存檔是否有效。"
                );

                return;
            }

            ShowSuccess("遊戲已讀取。");
            Close();
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

        SceneManager.LoadScene(SceneNames.Main);
    }
    private void HandleCancelPressed()
    {
        if (confirmDialog != null && confirmDialog.IsOpen)
        {
            confirmDialog.Hide();
            return;
        }

        if (windowRoot != null && windowRoot.activeSelf)
        {
            Close();
        }
    }
}
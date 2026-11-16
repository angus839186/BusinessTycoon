using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SaveSlotItem : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI detailText;
    [SerializeField] private TextMeshProUGUI actionText;

    [Header("Action")]
    [SerializeField] private Button actionButton;

    [SerializeField] private Button deleteButton;

    private string slotId;
    private Action<string> action;
    private Action<string> deleteAction;

    private void Awake()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(HandleActionClicked);
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(
                HandleDeleteClicked
            );
        }
    }

    private void OnDestroy()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(HandleActionClicked);
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(
                HandleDeleteClicked
            );
        }
    }

    public void Bind(
        SaveSlotInfo info,
        string actionLabel,
        bool actionEnabled,
        Action<string> onAction,
        bool deleteEnabled,
        Action<string> onDelete
    )
    {
        slotId = info != null
            ? info.SlotId
            : string.Empty;

        action = onAction;
        deleteAction = onDelete;

        if (nameText != null)
        {
            nameText.text = GetDisplayName(slotId);
        }

        if (actionText != null)
        {
            actionText.text = actionLabel;
        }

        if (actionButton != null)
        {
            actionButton.interactable =
                actionEnabled;
        }
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(
                deleteEnabled
            );

            deleteButton.interactable =
                deleteEnabled;
        }

        RefreshInfo(info);
    }

    private void RefreshInfo(SaveSlotInfo info)
    {
        if (info == null)
        {
            SetStatus(
                "無法取得存檔資料",
                string.Empty
            );

            return;
        }

        if (!info.Exists)
        {
            SetStatus(
                "空白存檔",
                "尚未建立存檔"
            );

            return;
        }

        if (!string.IsNullOrWhiteSpace(
            info.ErrorMessage
        ))
        {
            SetStatus(
                "存檔異常",
                info.ErrorMessage
            );

            return;
        }

        string details =
            $"遊戲進度：{info.TotalGameMinutes:0} 分鐘\n" +
            $"儲存時間：{info.SavedAt:yyyy/MM/dd HH:mm}\n" +
            $"版本：{info.SaveVersion}";

        SetStatus(
            info.CanLoad
                ? "可以讀取"
                : "版本不相容",
            details
        );
    }

    private void SetStatus(string status, string details)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }

        if (detailText != null)
        {
            detailText.text = details;
        }
    }

    private void HandleActionClicked()
    {
        if (string.IsNullOrWhiteSpace(slotId) || action == null) return;

        action.Invoke(slotId);
    }
    private void HandleDeleteClicked()
    {
        if (
            string.IsNullOrWhiteSpace(slotId) ||
            deleteAction == null
        )
        {
            return;
        }

        deleteAction.Invoke(slotId);
    }

    private string GetDisplayName(string id)
    {
        if (string.Equals(
        id, SaveSlots.AutoSaveId, StringComparison.Ordinal))
        {
            return "自動存檔";
        }
        const string prefix = "slot_";

        if (!string.IsNullOrWhiteSpace(id) && id.StartsWith(prefix, StringComparison.Ordinal))
        {
            string number = id.Substring(prefix.Length);
            return $"存檔 {number}";
        }

        return string.IsNullOrWhiteSpace(id) ? "未知存檔" : id;
    }
}
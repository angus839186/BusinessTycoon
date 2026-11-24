using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class SaveSlotItem : MonoBehaviour
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


}
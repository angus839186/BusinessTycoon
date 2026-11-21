using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ConfirmDialog : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private GameInput gameInput;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private UiTransition transition;


    private Action confirmAction;
    private Action cancelAction;
    private bool restoreMapInput;

    public bool IsOpen => transition != null && transition.IsVisible;

    private bool HasRequiredReferences =>
    titleText != null &&
    messageText != null &&
    confirmButton != null &&
    cancelButton != null &&
    closeButton != null &&
    gameInput != null &&
    transition != null;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }

        if (!HasRequiredReferences)
        {
            Debug.LogError("ConfirmDialog 缺少必要的 UI 或系統參考。", this);
            enabled = false;
            return;
        }

        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(HandleCancel);
        closeButton.onClick.AddListener(HandleCancel);

        transition.Hide(true);
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(HandleCancel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCancel);
        }
    }

    public void Show(string title, string message, Action onConfirm, Action onCancel = null)
    {
        if (!IsOpen)
        {
            restoreMapInput = gameInput.IsMapEnabled;
        }

        confirmAction = onConfirm;
        cancelAction = onCancel;

        titleText.text = title;
        messageText.text = message;

        gameInput.SetMapEnabled(false);
        transition.Show();
    }

    public void Hide()
    {
        transition.Hide();
        gameInput.SetMapEnabled(restoreMapInput);

        confirmAction = null;
        cancelAction = null;
    }

    private void HandleConfirm()
    {
        Action action = confirmAction;

        Hide();
        action?.Invoke();
    }

    private void HandleCancel()
    {
        Action action = cancelAction;

        Hide();
        action?.Invoke();
    }
}
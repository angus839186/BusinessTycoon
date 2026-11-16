using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ConfirmDialog : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button closeButton;

    [Header("Input")]
    [SerializeField] private GameInput gameInput;

    private Action confirmAction;
    private Action cancelAction;
    private bool restoreMapInput;

    public bool IsOpen => windowRoot != null && windowRoot.activeSelf;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HandleConfirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(HandleCancel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleCancel);
        }

        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }
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

    public void Show(
        string title,
        string message,
        Action onConfirm,
        Action onCancel = null
    )
    {
        if (windowRoot == null)
        {
            Debug.LogError(
                "ConfirmDialog 尚未設定 Window Root。",
                this
            );

            return;
        }

        if (!IsOpen)
        {
            restoreMapInput = gameInput != null && gameInput.IsMapEnabled;
        }

        confirmAction = onConfirm;
        cancelAction = onCancel;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (gameInput != null)
        {
            gameInput.SetMapEnabled(false);
        }

        windowRoot.SetActive(true);
    }

    public void Hide()
    {
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }

        if (gameInput != null)
        {
            gameInput.SetMapEnabled(restoreMapInput);
        }

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
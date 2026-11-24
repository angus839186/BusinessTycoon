using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum SaveSlotMode
{
    Load,
    Save
}

public sealed partial class SaveSlotPanel : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameSaveSystem gameSaveSystem;
    [SerializeField] private LoadingController loadingController;

    [Header("UI")]
    [SerializeField] private NoticeUI noticeUI;
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform listRoot;
    [SerializeField] private SaveSlotItem itemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [Header("Dialog")][SerializeField] private ConfirmDialog confirmDialog;
    public event Action SlotsChanged;
    private SaveSlotMode mode;
    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }
        if (gameSaveSystem == null)
        {
            gameSaveSystem = FindFirstObjectByType<GameSaveSystem>();
        }

        if (loadingController == null)
        {
            loadingController = FindFirstObjectByType<LoadingController>();
        }

        if (itemPrefab != null && listRoot != null)
        {
            itemList = new ReusableUIList<SaveSlotItem>(itemPrefab, listRoot);
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
            noticeUI = FindFirstObjectByType<NoticeUI>();
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
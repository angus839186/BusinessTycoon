using UnityEngine;
using UnityEngine.UI;
using System;

public sealed partial class PanelController : MonoBehaviour
{
    [SerializeField] private Button menuToggleButton;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private Transform panelRoot;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private bool openFirstPanelOnStart;
    [SerializeField] private GameInput gameInput;

    [Header("Menu")]
    [SerializeField] private UiTransition menuTransition;
    public bool HasOpenPanel => currentPanel != null;
    public event Action<bool> PanelStateChanged;

    private GameObject currentPanel;

    private bool HasRequiredReferences =>
    menuToggleButton != null &&
    buttonPrefab != null &&
    closeButton != null &&
    buttonRoot != null &&
    panelRoot != null &&
    panels != null &&
    panels.Length > 0 &&
    gameInput != null &&
    menuTransition != null;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }

        if (!HasRequiredReferences)
        {
            Debug.LogError("PanelController 缺少必要的 UI 或系統參考。", this);
            enabled = false;
            return;
        }

        menuTransition.Hide(true);
    }

    private void OnEnable()
    {
        gameInput.CancelPressed += HandleCancelPressed;
        gameInput.MenuTogglePressed += HandleMenuTogglePressed;
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed -= HandleCancelPressed;
            gameInput.MenuTogglePressed -= HandleMenuTogglePressed;
        }
    }

    private void Start()
    {
        CreateButtons();

        closeButton.onClick.AddListener(CloseCurrentPanel);
        menuToggleButton.onClick.AddListener(ToggleMenu);

        CloseCurrentPanel();

        if (openFirstPanelOnStart && panels[0] != null)
        {
            OpenPanel(panels[0]);
        }
    }

    private void HandleCancelPressed()
    {
        if (currentPanel != null)
        {
            CloseCurrentPanel();
            return;
        }

        if (menuTransition.IsVisible)
        {
            SetMenuOpen(false);
        }
    }


    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseCurrentPanel);
        }

        if (menuToggleButton != null)
        {
            menuToggleButton.onClick.RemoveListener(ToggleMenu);
        }
    }

    private void HandleMenuTogglePressed()
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (currentPanel != null)
        {
            CloseCurrentPanel();
            SetMenuOpen(true);
            return;
        }

        SetMenuOpen(!menuTransition.IsVisible);
    }

    private void SetMenuOpen(bool open, bool instant = false)
    {
        menuTransition.SetVisible(open, instant);
    }


}
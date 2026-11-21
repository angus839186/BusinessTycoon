using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public sealed class PanelController : MonoBehaviour
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
    private readonly Dictionary<GameObject, GameObject> panelInstances =
    new Dictionary<GameObject, GameObject>();
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

    private void CreateButtons()
    {

        foreach (GameObject panel in panels)
        {
            if (panel == null) continue;

            GameObject targetPanel = panel;
            Button button = Instantiate(buttonPrefab, buttonRoot);
            PanelButton buttonUI = button.GetComponent<PanelButton>();

            if (buttonUI != null)
            {
                buttonUI.SetContent(targetPanel);
            }
            else
            {
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null) label.text = targetPanel.name;
            }

            button.onClick.AddListener(() => OpenPanel(targetPanel));
            button.gameObject.SetActive(true);
        }
    }

    public void OpenPanel(GameObject panelPrefab)
    {
        if (panelPrefab == null) return;

        bool wasOpen = currentPanel != null;

        SetAllPanelsInactive();

        if (!panelInstances.TryGetValue(panelPrefab, out GameObject panelInstance) || panelInstance == null)
        {
            panelInstance = Instantiate(panelPrefab, panelRoot, false);
            panelInstances[panelPrefab] = panelInstance;
        }

        currentPanel = panelInstance;
        currentPanel.SetActive(true);

        gameInput.SetMapEnabled(false);

        if (!wasOpen) PanelStateChanged?.Invoke(true);

        SetMenuOpen(false);

        closeButton.gameObject.SetActive(true);
    }

    public void CloseCurrentPanel()
    {
        bool wasOpen = currentPanel != null;
        SetAllPanelsInactive();
        currentPanel = null;
        gameInput.SetMapEnabled(true);
        if (wasOpen) PanelStateChanged?.Invoke(false);
        closeButton.gameObject.SetActive(false);

        SetMenuOpen(false);
    }

    private void SetAllPanelsInactive()
    {
        foreach (GameObject panelInstance in panelInstances.Values)
        {
            if (panelInstance != null) panelInstance.SetActive(false);
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public sealed class PanelController : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private Transform panelRoot;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private bool openFirstPanelOnStart;

    [SerializeField] private GameInput gameInput;

    public bool HasOpenPanel =>
    currentPanel != null;

    public event Action<bool> PanelStateChanged;

    private readonly Dictionary<GameObject, GameObject> panelInstances =
    new Dictionary<GameObject, GameObject>();

    private GameObject currentPanel;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput =
                FindFirstObjectByType<GameInput>();
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed += HandleCancelPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed -= HandleCancelPressed;
        }
    }

    private void Start()
    {
        CreateButtons();

        if (closeButton != null) closeButton.onClick.AddListener(CloseCurrentPanel);

        CloseCurrentPanel();

        if (openFirstPanelOnStart &&
        panels != null && panels.Length > 0 && panels[0] != null)
        {
            OpenPanel(panels[0]);
        }
    }

    private void HandleCancelPressed()
    {
        if (currentPanel != null)
        {
            CloseCurrentPanel();
        }
    }


    private void OnDestroy()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseCurrentPanel);
    }

    private void CreateButtons()
    {
        if (buttonPrefab == null || buttonRoot == null || panels == null)
            return;

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
        if (panelPrefab == null)
            return;

        if (panelRoot == null)
        {
            Debug.LogError("PanelController 尚未設定 Panel Root。", this);
            return;
        }

        bool wasOpen = currentPanel != null;

        SetAllPanelsInactive();

        if (!panelInstances.TryGetValue(panelPrefab, out GameObject panelInstance)
            || panelInstance == null)
        {
            panelInstance = Instantiate(panelPrefab, panelRoot, false);

            panelInstances[panelPrefab] = panelInstance;
        }

        currentPanel = panelInstance;
        currentPanel.SetActive(true);

        if (gameInput != null) gameInput.SetMapEnabled(false);

        if (!wasOpen) PanelStateChanged?.Invoke(true);

        if (buttonRoot != null) buttonRoot.gameObject.SetActive(false);

        if (closeButton != null) closeButton.gameObject.SetActive(true);
    }

    public void CloseCurrentPanel()
    {
        bool wasOpen = currentPanel != null;
        SetAllPanelsInactive();
        currentPanel = null;
        if (gameInput != null) gameInput.SetMapEnabled(true);

        if (wasOpen) PanelStateChanged?.Invoke(false);

        if (closeButton != null) closeButton.gameObject.SetActive(false);

        if (buttonRoot != null) buttonRoot.gameObject.SetActive(true);
    }

    private void SetAllPanelsInactive()
    {
        foreach (GameObject panelInstance in panelInstances.Values)
        {
            if (panelInstance != null) panelInstance.SetActive(false);
        }
    }
}
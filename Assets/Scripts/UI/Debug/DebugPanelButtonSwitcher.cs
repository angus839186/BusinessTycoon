using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class DebugPanelButtonSwitcher : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject[] panels;

    private GameObject currentPanel;

    private void Start()
    {
        CreateButtons();
        CloseCurrentPanel();

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCurrentPanel);
    }

    private void Update()
    {
        if (
            currentPanel != null &&
            Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame
        )
        {
            CloseCurrentPanel();
        }
    }

    private void CreateButtons()
    {
        if (buttonPrefab == null || buttonRoot == null || panels == null)
            return;

        foreach (GameObject panel in panels)
        {
            if (panel == null)
                continue;

            Button button = Instantiate(buttonPrefab, buttonRoot);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = panel.name;
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() => OpenPanel(panel));
        }
    }

    private void OpenPanel(GameObject panel)
    {
        CloseCurrentPanel();
        currentPanel = panel;
        currentPanel.SetActive(true);
        buttonRoot.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(true);
    }

    private void CloseCurrentPanel()
    {
        if (panels != null)
        {
            foreach (GameObject panel in panels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }

        currentPanel = null;
        closeButton.gameObject.SetActive(false);
        buttonRoot.gameObject.SetActive(true);
    }
}
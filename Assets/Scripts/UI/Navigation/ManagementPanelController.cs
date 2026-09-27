using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class ManagementPanelController : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private bool openFirstPanelOnStart;

    private GameObject currentPanel;

    private void Start()
    {
        CreateButtons();

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCurrentPanel);

        CloseCurrentPanel();

        if (
            openFirstPanelOnStart &&
            panels != null &&
            panels.Length > 0 &&
            panels[0] != null
        )
        {
            OpenPanel(panels[0]);
        }
    }
    private void Update()
    {
        if (
            currentPanel == null ||
            Keyboard.current == null ||
            !Keyboard.current.escapeKey.wasPressedThisFrame
        )
        {
            return;
        }

        CloseCurrentPanel();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseCurrentPanel);
    }

    private void CreateButtons()
    {
        if (
            buttonPrefab == null ||
            buttonRoot == null ||
            panels == null
        )
        {
            return;
        }

        foreach (GameObject panel in panels)
        {
            if (panel == null)
                continue;

            GameObject targetPanel = panel;
            Button button = Instantiate(buttonPrefab, buttonRoot);

            ManagementPanelButtonUI buttonUI =
    button.GetComponent<ManagementPanelButtonUI>();

            if (buttonUI != null)
            {
                buttonUI.SetContent(targetPanel);
            }
            else
            {
                TextMeshProUGUI label =
                    button.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null)
                    label.text = targetPanel.name;
            }

            button.onClick.AddListener(
                () => OpenPanel(targetPanel)
            );

            button.gameObject.SetActive(true);
        }
    }

    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
            return;

        SetAllPanelsInactive();

        currentPanel = panel;
        currentPanel.SetActive(true);

        if (buttonRoot != null)
            buttonRoot.gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    public void CloseCurrentPanel()
    {
        SetAllPanelsInactive();
        currentPanel = null;

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (buttonRoot != null)
            buttonRoot.gameObject.SetActive(true);
    }

    private void SetAllPanelsInactive()
    {
        if (panels == null)
            return;

        foreach (GameObject panel in panels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
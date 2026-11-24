using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class PanelController
{
    private readonly Dictionary<GameObject, GameObject> panelInstances = new Dictionary<GameObject, GameObject>();

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
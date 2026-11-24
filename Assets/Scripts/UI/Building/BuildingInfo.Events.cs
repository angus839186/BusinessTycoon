public sealed partial class BuildingInfo
{
    private void OnEnable()
    {
        selector.SelectionChanged += HandleSelectionChanged;

        productionSystem.DataChanged += Refresh;
        gameClock.TimeChanged += Refresh;
        panelController.PanelStateChanged += HandlePanelStateChanged;
        panelOpen = panelController.HasOpenPanel;
        closeButton.onClick.AddListener(HandleCloseClicked);

        HandleSelectionChanged(selector.SelectedBuilding);
    }

    private void OnDisable()
    {
        if (selector != null)
        {
            selector.SelectionChanged -= HandleSelectionChanged;
        }

        if (productionSystem != null)
        {
            productionSystem.DataChanged -= Refresh;
        }

        if (gameClock != null)
        {
            gameClock.TimeChanged -= Refresh;
        }

        if (panelController != null)
        {
            panelController.PanelStateChanged -= HandlePanelStateChanged;
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        UnsubscribeBuilding();
        selectedBuilding = null;
        if (transition != null)
        {
            transition.Hide(true);
        }
    }
    private void HandleCloseClicked()
    {
        selector.ClearSelection();
    }

    private void HandleSelectionChanged(BuildingInstance building)
    {
        UnsubscribeBuilding();
        selectedBuilding = building;

        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged += HandleOperationalChanged;
        }

        Refresh();
    }

    private void HandleOperationalChanged(bool operational
    )
    {
        Refresh();
    }

    private void HandlePanelStateChanged(bool isOpen)
    {
        panelOpen = isOpen;
        Refresh();
    }

    private void UnsubscribeBuilding()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged -= HandleOperationalChanged;
        }
    }
}
using UnityEngine;

public enum MapMode
{
    None,
    Placement,
    Demolition,
    Route
}

public sealed class MapModeController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PanelController panelManager;
    [SerializeField] private BuildingPlacementController placement;
    [SerializeField] private BuildingDemolitionController demolition;
    [SerializeField] private BuildingSelector buildingSelector;
    [SerializeField] private TransportRouteBuilder routeBuilder;

    private bool initialized;

    public MapMode CurrentMode
    {
        get
        {
            if (placement.IsPlacementMode)
                return MapMode.Placement;

            if (demolition.IsDemolitionMode)
                return MapMode.Demolition;

            if (routeBuilder.IsCreatingRoute)
                return MapMode.Route;

            return MapMode.None;
        }
    }

    public bool HasActiveMode => CurrentMode != MapMode.None;

    private bool HasRequiredReferences =>
        gameInput != null && panelManager != null && placement != null &&
        demolition != null && buildingSelector != null && routeBuilder != null;

    private void Awake()
    {
        if (!HasRequiredReferences)
        {
            Debug.LogError("MapModeController 缺少必要參考。", this);
            enabled = false;
            return;
        }

        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        panelManager.PanelStateChanged += HandlePanelStateChanged;
        gameInput.CancelPressed += HandleCancelInput;
        gameInput.SecondaryPressed += HandleCancelInput;
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        panelManager.PanelStateChanged -= HandlePanelStateChanged;
        gameInput.CancelPressed -= HandleCancelInput;
        gameInput.SecondaryPressed -= HandleCancelInput;
    }

    public void StartPlacementMode()
    {
        CancelMapMode();
        ClearBuildingSelection();
        placement.EnterPlacementMode();
    }

    public void ToggleDemolitionMode()
    {
        if (CurrentMode == MapMode.Demolition)
        {
            CancelMapMode();
            return;
        }

        CancelMapMode();
        ClearBuildingSelection();
        demolition.EnterDemolitionMode();
    }

    public void StartRouteMode()
    {
        CancelMapMode();
        ClearBuildingSelection();
        routeBuilder.StartCreateRoute();
    }

    public void CancelMapMode()
    {
        if (placement.IsPlacementMode)
            placement.ExitPlacementMode();

        if (demolition.IsDemolitionMode)
            demolition.ExitDemolitionMode();

        if (routeBuilder.IsCreatingRoute)
            routeBuilder.CancelCreateRoute();
    }

    private void HandleCancelInput()
    {
        if (HasActiveMode)
            CancelMapMode();
    }

    private void HandlePanelStateChanged(bool isOpen)
    {
        if (isOpen)
            CancelMapMode();
    }

    private void ClearBuildingSelection()
    {
        buildingSelector.ClearSelection();
    }
}
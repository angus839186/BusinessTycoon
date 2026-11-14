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

    [SerializeField] private TransportRouteBuilder routeBuilder;

    public MapMode CurrentMode
    {
        get
        {
            if (placement != null && placement.IsPlacementMode)
                return MapMode.Placement;

            if (demolition != null && demolition.IsDemolitionMode)
                return MapMode.Demolition;

            if (routeBuilder != null && routeBuilder.IsCreatingRoute)
                return MapMode.Route;

            return MapMode.None;
        }
    }

    public bool HasActiveMode =>
        CurrentMode != MapMode.None;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }
        if (panelManager == null)
            panelManager = FindFirstObjectByType<PanelController>();

        if (placement == null)
            placement = FindFirstObjectByType<BuildingPlacementController>();

        if (demolition == null)
            demolition = FindFirstObjectByType<BuildingDemolitionController>();

        if (routeBuilder == null)
            routeBuilder = FindFirstObjectByType<TransportRouteBuilder>();
    }

    private void OnEnable()
    {
        if (panelManager != null)
        {
            panelManager.PanelStateChanged += HandlePanelStateChanged;
        }

        if (gameInput != null)
        {
            gameInput.CancelPressed += HandleCancelInput;

            gameInput.SecondaryPressed += HandleCancelInput;
        }
    }

    private void OnDisable()
    {
        if (panelManager != null)
        {
            panelManager.PanelStateChanged -= HandlePanelStateChanged;
        }
        if (gameInput != null)
        {
            gameInput.CancelPressed -= HandleCancelInput;
            gameInput.SecondaryPressed -= HandleCancelInput;
        }
    }

    public void StartPlacementMode()
    {
        CancelMapMode();

        if (placement != null)
        {
            placement.EnterPlacementMode();
        }
    }

    public void ToggleDemolitionMode()
    {
        if (CurrentMode == MapMode.Demolition)
        {
            CancelMapMode();
            return;
        }

        CancelMapMode();

        if (demolition != null)
        {
            demolition.EnterDemolitionMode();
        }
    }

    public void StartRouteMode()
    {
        CancelMapMode();

        if (routeBuilder != null)
        {
            routeBuilder.StartCreateRoute();
        }
    }

    private void HandleCancelInput()
    {
        if (!HasActiveMode) return;
        CancelMapMode();
    }

    private void HandlePanelStateChanged(bool isOpen)
    {
        if (isOpen)
            CancelMapMode();
    }

    public void CancelMapMode()
    {
        if (placement != null && placement.IsPlacementMode)
            placement.ExitPlacementMode();

        if (demolition != null && demolition.IsDemolitionMode)
            demolition.ExitDemolitionMode();

        if (routeBuilder != null && routeBuilder.IsCreatingRoute)
            routeBuilder.CancelCreateRoute();
    }
}
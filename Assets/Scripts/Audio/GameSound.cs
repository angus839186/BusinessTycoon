using UnityEngine;

public sealed class GameSound : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController buildingPlacement;

    [SerializeField] private TransportSystem transportSystem;

    [SerializeField] private TransportRouteBuilder routeBuilder;

    [SerializeField] private MapPointer mapPointer;

    private bool HasRequiredReferences =>
    buildingPlacement != null &&
    transportSystem != null &&
    routeBuilder != null &&
    mapPointer != null;

    private void Awake()
    {
        if (HasRequiredReferences)
        {
            return;
        }

        Debug.LogError("GameSound 缺少必要的系統參考。", this);
        enabled = false;
    }

    private void OnEnable()
    {
        buildingPlacement.BuildingPlaced += HandleBuildingPlaced;
        buildingPlacement.PlacementFailed += HandleOperationFailed;

        transportSystem.RouteCreated += HandleRouteCreated;
        routeBuilder.RouteFailed += HandleOperationFailed;

        mapPointer.MapClicked += HandleMapClicked;
        mapPointer.InvalidPlacementClicked += HandleInvalidPlacement;
    }

    private void OnDisable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.BuildingPlaced -= HandleBuildingPlaced;
            buildingPlacement.PlacementFailed -= HandleOperationFailed;
        }

        if (transportSystem != null)
        {
            transportSystem.RouteCreated -= HandleRouteCreated;
        }

        if (routeBuilder != null)
        {
            routeBuilder.RouteFailed -= HandleOperationFailed;
        }

        if (mapPointer != null)
        {
            mapPointer.MapClicked -= HandleMapClicked;
            mapPointer.InvalidPlacementClicked -= HandleInvalidPlacement;
        }
    }

    private static void PlaySfx(SfxId id)
    {
        AudioSystem audioSystem = AudioSystem.Instance;

        if (audioSystem == null)
        {
            return;
        }

        audioSystem.PlaySfx(id);
    }

    private void HandleBuildingPlaced(BuildingInstance building)
    {
        PlaySfx(SfxId.BuildingPlaced);
    }

    private void HandleRouteCreated(TransportRouteDefinition route)
    {
        PlaySfx(SfxId.RouteCreated);
    }
    private void HandleOperationFailed(string message)
    {
        PlaySfx(SfxId.OperationFailed);
    }

    private void HandleMapClicked()
    {
        PlaySfx(SfxId.MapClick);
    }

    private void HandleInvalidPlacement()
    {
        PlaySfx(SfxId.OperationFailed);
    }
}
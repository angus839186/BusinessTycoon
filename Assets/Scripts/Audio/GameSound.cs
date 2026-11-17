using UnityEngine;

public sealed class GameSound : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController buildingPlacement;

    [SerializeField] private TransportSystem transportSystem;

    [SerializeField] private TransportRouteBuilder routeBuilder;

    [SerializeField] private MapPointer mapPointer;

    private void Awake()
    {
        if (buildingPlacement == null)
        {
            buildingPlacement = FindFirstObjectByType<BuildingPlacementController>();
        }

        if (transportSystem == null)
        {
            transportSystem = FindFirstObjectByType<TransportSystem>();
        }
        if (routeBuilder == null)
        {
            routeBuilder = FindFirstObjectByType<TransportRouteBuilder>();
        }
        if (mapPointer == null)
        {
            mapPointer =
                FindFirstObjectByType<MapPointer>();
        }
    }

    private void OnEnable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.BuildingPlaced += HandleBuildingPlaced;
        }

        if (transportSystem != null)
        {
            transportSystem.RouteCreated += HandleRouteCreated;
        }
        if (buildingPlacement != null)
        {
            buildingPlacement.PlacementFailed += HandleOperationFailed;
        }

        if (routeBuilder != null)
        {
            routeBuilder.RouteFailed += HandleOperationFailed;
        }
        if (mapPointer != null)
        {
            mapPointer.MapClicked += HandleMapClicked;

            mapPointer.InvalidPlacementClicked += HandleInvalidPlacement;
        }
    }

    private void OnDisable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.BuildingPlaced -= HandleBuildingPlaced;
        }

        if (transportSystem != null)
        {
            transportSystem.RouteCreated -= HandleRouteCreated;
        }
        if (buildingPlacement != null)
        {
            buildingPlacement.PlacementFailed -= HandleOperationFailed;
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

    private void HandleBuildingPlaced(BuildingInstance building)
    {
        AudioSystem.Instance?.PlaySfx(SfxId.BuildingPlaced);
    }

    private void HandleRouteCreated(TransportRouteDefinition route)
    {
        AudioSystem.Instance?.PlaySfx(SfxId.RouteCreated);
    }
    private void HandleOperationFailed(string message)
    {
        AudioSystem.Instance?.PlaySfx(SfxId.OperationFailed);
    }

    private void HandleMapClicked()
    {
        AudioSystem.Instance?.PlaySfx(
            SfxId.MapClick
        );
    }

    private void HandleInvalidPlacement()
    {
        AudioSystem.Instance?.PlaySfx(
            SfxId.OperationFailed
        );
    }
}
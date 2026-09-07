using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MapPointer : MonoBehaviour
{
    [SerializeField] private GeoJsonMapProjection geoJsonProjection;
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private BuildingPlacementPreview placementPreview;

    [SerializeField] private BuildableAreaQuery buildableAreaQuery;
    [SerializeField] private BuildingDemolitionController demolitionController;

    private void Update()
    {
        if (geoJsonProjection == null || Mouse.current == null)
            return;

        Camera camera = Camera.main;
        if (demolitionController != null && demolitionController.IsDemolitionMode)
        {
            if (placementPreview != null)
                placementPreview.Hide();

            return;
        }

        if (camera == null)
            return;

        bool clicked = Mouse.current.leftButton.wasPressedThisFrame;

        if (
    buildingPlacement != null &&
    buildingPlacement.IsPlacementMode &&
    (
        Mouse.current.rightButton.wasPressedThisFrame ||
        Keyboard.current.escapeKey.wasPressedThisFrame
    )
)
        {
            buildingPlacement.ExitPlacementMode();

            if (placementPreview != null)
                placementPreview.Hide();

            return;
        }
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        if (!geoJsonProjection.TryWorldPositionToLonLat(
            worldPosition,
            out double longitude,
            out double latitude
        ))
        {
            if (placementPreview != null)
                placementPreview.Hide();

            return;
        }

        bool isInsideBuildableArea =
    buildableAreaQuery != null &&
    buildableAreaQuery.Contains(longitude, latitude);

        if (buildingPlacement == null || !buildingPlacement.IsPlacementMode)
        {
            if (placementPreview != null)
                placementPreview.Hide();

            return;
        }

        bool canBuild =
            isInsideBuildableArea &&
            buildingPlacement != null &&
            buildingPlacement.CanPlaceSelectedBuilding();

        if (placementPreview != null)
            placementPreview.Show(worldPosition, canBuild);

        if (!clicked)
            return;

        if (!canBuild)
            return;

        Debug.Log($"Map Click: Lon={longitude:F6}, Lat={latitude:F6}");

        if (buildingPlacement != null)
        {
            buildingPlacement.PlaceBuilding(
                worldPosition,
                longitude,
                latitude
            );
        }
    }
}
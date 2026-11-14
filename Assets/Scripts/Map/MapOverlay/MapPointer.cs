using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MapPointer : MonoBehaviour
{
    [SerializeField]
    private GameInput gameInput;

    [SerializeField] private GeoJsonMapProjection geoJsonProjection;
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private BuildingPlacementPreview placementPreview;

    [SerializeField] private BuildableAreaQuery buildableAreaQuery;
    private bool primaryPressed;

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
            gameInput.PrimaryPressed +=
                HandlePrimaryPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed -=
                HandlePrimaryPressed;
        }

        primaryPressed = false;
    }

    private void HandlePrimaryPressed()
    {
        primaryPressed = true;
    }

    private void Update()
    {

        bool clicked = primaryPressed;
        primaryPressed = false;

        if (geoJsonProjection == null || gameInput == null) return;

        Camera camera = Camera.main;

        if (camera == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (placementPreview != null) placementPreview.Hide();
            return;
        }

        Vector2 screenPosition = gameInput.PointerPosition;
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        if (!geoJsonProjection.TryWorldPositionToLonLat(
            worldPosition,
            out double longitude,
            out double latitude
        ))
        {
            if (placementPreview != null) placementPreview.Hide();
            return;
        }

        bool isInsideBuildableArea =
    buildableAreaQuery != null && buildableAreaQuery.Contains(longitude, latitude);

        if (buildingPlacement == null || !buildingPlacement.IsPlacementMode)
        {
            if (placementPreview != null) placementPreview.Hide();

            return;
        }

        bool canBuild =
            isInsideBuildableArea &&
            buildingPlacement != null &&
            buildingPlacement.CanPlaceSelectedBuilding();

        if (placementPreview != null) placementPreview.Show(worldPosition, canBuild);

        if (!clicked) return;

        if (!canBuild) return;

        Debug.Log($"Map Click: Lon={longitude:F6}, Lat={latitude:F6}");

        if (buildingPlacement != null)
        {
            buildingPlacement.PlaceBuilding(worldPosition, longitude, latitude);
        }
    }
}
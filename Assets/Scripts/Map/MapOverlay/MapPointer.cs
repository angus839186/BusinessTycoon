using UnityEngine;
using UnityEngine.EventSystems;
using System;

public sealed class MapPointer : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private MapModeController mapMode;

    [SerializeField] private GeoJsonMapProjection geoJsonProjection;
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private BuildingPlacementPreview placementPreview;

    [SerializeField] private BuildableAreaQuery buildableAreaQuery;
    private bool primaryPressed;
    public event Action MapClicked;
    public event Action InvalidPlacementClicked;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }
        if (mapMode == null)
        {
            mapMode = FindFirstObjectByType<MapModeController>();
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed += HandlePrimaryPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed -= HandlePrimaryPressed;
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

        if (
    buildingPlacement == null ||
    !buildingPlacement.IsPlacementMode
)
        {
            if (placementPreview != null)
            {
                placementPreview.Hide();
            }

            bool hasOtherMapMode =
                mapMode != null &&
                mapMode.HasActiveMode;

            if (
                clicked &&
                !hasOtherMapMode &&
                !IsPointerOverBuilding(
                    screenPosition
                )
            )
            {
                MapClicked?.Invoke();
            }

            return;
        }

        bool canBuild =
            isInsideBuildableArea &&
            buildingPlacement != null &&
            buildingPlacement.CanPlaceSelectedBuilding();

        if (placementPreview != null) placementPreview.Show(worldPosition, canBuild);

        if (!clicked)
        {
            return;
        }

        if (!canBuild)
        {
            InvalidPlacementClicked?.Invoke();
            return;
        }

        Debug.Log($"Map Click: Lon={longitude:F6}, Lat={latitude:F6}");

        if (buildingPlacement != null)
        {
            buildingPlacement.PlaceBuilding(worldPosition, longitude, latitude);
        }
    }
    private bool IsPointerOverBuilding(Vector2 screenPosition)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            return false;
        }

        Ray ray = camera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return false;
        }

        return hit.collider.GetComponentInParent<BuildingInstance>() != null;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class BuildingSelectionController : MonoBehaviour
{
    [SerializeField] private BuildingInfoPanel infoPanel;
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private BuildingDemolitionController demolitionController;
    [SerializeField] private TransportRouteBuilder routeBuilder;

    private void Awake()
    {
        if (infoPanel == null)
            infoPanel = FindFirstObjectByType<BuildingInfoPanel>();

        if (buildingPlacement == null)
            buildingPlacement = FindFirstObjectByType<BuildingPlacementController>();

        if (demolitionController == null)
            demolitionController = FindFirstObjectByType<BuildingDemolitionController>();

        if (routeBuilder == null)
            routeBuilder = FindFirstObjectByType<TransportRouteBuilder>();
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (buildingPlacement != null && buildingPlacement.IsPlacementMode)
            return;

        if (demolitionController != null && demolitionController.IsDemolitionMode)
            return;
        if (routeBuilder != null && routeBuilder.IsCreatingRoute)
            return;

        BuildingInstance building = RaycastBuilding();

        if (building == null)
        {
            if (infoPanel != null)
                infoPanel.ClearSelection();

            return;
        }

        if (infoPanel != null)
            infoPanel.SetBuilding(building);
    }

    private BuildingInstance RaycastBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return null;

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        return hit.collider.GetComponent<BuildingInstance>();
    }
}
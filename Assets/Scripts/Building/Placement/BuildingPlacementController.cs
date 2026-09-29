using UnityEngine;
using System;

public sealed partial class BuildingPlacementController : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private BuildingDefinition selectedBuilding;
    [SerializeField] private BuildingDemolitionController demolitionController;

    public event Action<BuildingDefinition> SelectedBuildingChanged;
    public event Action<bool> PlacementModeChanged;
    public BuildingDefinition SelectedBuilding => selectedBuilding;
    private int nextBuildingInstanceIndex = 1;
    public int NextBuildingInstanceIndex => nextBuildingInstanceIndex;

    private bool isPlacementMode;

    public bool IsPlacementMode => isPlacementMode;

    private void Awake()
    {
        if (demolitionController == null)
            demolitionController = FindFirstObjectByType<BuildingDemolitionController>();
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
    }


    public void SelectBuilding(BuildingDefinition building)
    {
        if (selectedBuilding == building)
        {
            return;
        }

        selectedBuilding = building;
        SelectedBuildingChanged?.Invoke(selectedBuilding);

        if (selectedBuilding == null)
        {
            Debug.Log("Building selected: none");
            return;
        }

        Debug.Log(
            $"Building selected: {selectedBuilding.displayName}"
        );
    }

    public void EnterPlacementMode()
    {
        if (selectedBuilding == null)
        {
            Debug.Log(
                "Cannot enter placement mode: no building selected."
            );

            return;
        }

        if (isPlacementMode)
        {
            return;
        }

        if (demolitionController != null &&
            demolitionController.IsDemolitionMode)
        {
            demolitionController.ExitDemolitionMode();
        }

        isPlacementMode = true;
        PlacementModeChanged?.Invoke(true);

        Debug.Log(
            $"Placement mode entered: " +
            $"{selectedBuilding.displayName}"
        );
    }
    public void ExitPlacementMode()
    {
        if (!isPlacementMode)
        {
            return;
        }

        isPlacementMode = false;
        PlacementModeChanged?.Invoke(false);

        Debug.Log("Placement mode exited.");
    }

    public void TogglePlacementMode()
    {
        if (isPlacementMode)
            ExitPlacementMode();
        else
            EnterPlacementMode();
    }
    public bool CanPlaceSelectedBuilding()
    {
        return isPlacementMode && selectedBuilding != null;
    }

}
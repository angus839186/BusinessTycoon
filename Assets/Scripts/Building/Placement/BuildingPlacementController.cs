using UnityEngine;
using System.Collections.Generic;

public sealed partial class BuildingPlacementController : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private BuildingDefinition selectedBuilding;
    [SerializeField] private BuildingDemolitionController demolitionController;
    public BuildingDefinition SelectedBuilding => selectedBuilding;
    private int nextBuildingInstanceIndex = 1;
    public int NextBuildingInstanceIndex => nextBuildingInstanceIndex;
    private readonly List<PlacedBuildingData> placedBuildings =
    new List<PlacedBuildingData>();

    private bool isPlacementMode;

    public bool IsPlacementMode => isPlacementMode;

    [SerializeField] private string saveFileName = "placed_buildings.json";
    private void Awake()
    {
        if (demolitionController == null)
            demolitionController = FindFirstObjectByType<BuildingDemolitionController>();
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
    }


    public void SelectBuilding(BuildingDefinition building)
    {
        selectedBuilding = building;

        if (selectedBuilding == null)
        {
            Debug.Log("Building selected: none");
            return;
        }

        Debug.Log($"Building selected: {selectedBuilding.displayName}");
    }

    public void EnterPlacementMode()
    {
        if (selectedBuilding == null)
        {
            Debug.Log("Cannot enter placement mode: no building selected.");
            return;
        }

        isPlacementMode = true;
        Debug.Log($"Placement mode entered: {selectedBuilding.displayName}");
    }

    public void ExitPlacementMode()
    {
        isPlacementMode = false;
        Debug.Log("Placement mode exited.");
    }

    public void TogglePlacementMode()
    {
        if (isPlacementMode)
            ExitPlacementMode();
        else
            EnterPlacementMode();
    }

}
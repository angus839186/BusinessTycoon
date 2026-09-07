using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public sealed class BuildingSelectorUI : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private string resourcesFolder = "Buildings";

    private readonly List<BuildingDefinition> buildings = new List<BuildingDefinition>();

    private void Start()
    {
        LoadOptions();
    }

    private void LoadOptions()
    {
        if (buildingPlacement == null || dropdown == null)
        {
            Debug.LogError("BuildingSelectorUI missing references.");
            return;
        }

        BuildingDefinition[] loadedBuildings =
            Resources.LoadAll<BuildingDefinition>(resourcesFolder);

        buildings.Clear();
        dropdown.ClearOptions();

        List<string> labels = new List<string>();

        foreach (BuildingDefinition building in loadedBuildings)
        {
            if (building == null)
                continue;

            buildings.Add(building);
            labels.Add(string.IsNullOrWhiteSpace(building.displayName)
                ? building.name
                : building.displayName);
        }

        dropdown.AddOptions(labels);
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (buildings.Count > 0)
            buildingPlacement.SelectBuilding(buildings[dropdown.value]);
    }

    private void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= buildings.Count)
            return;

        buildingPlacement.SelectBuilding(buildings[index]);
    }
}
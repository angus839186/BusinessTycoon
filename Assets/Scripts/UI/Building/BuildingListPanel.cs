using UnityEngine;
using System;
using TMPro;

public sealed class BuildingListPanel : MonoBehaviour
{
    [SerializeField] private MapCameraController mapCamera;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private BuildingSelector buildingSelector;
    [SerializeField] private PanelController panelController;

    [SerializeField] private Transform listRoot;
    [SerializeField] private BuildingListItem itemPrefab;
    [SerializeField] private GameObject emptyText;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private TMP_Dropdown categoryDropdown;


    private ReusableUIList<BuildingListItem> itemList;

    private void Awake()
    {
        if (mapCamera == null)
        {
            mapCamera = FindFirstObjectByType<MapCameraController>();
        }
        if (buildingSelector == null)
        {
            buildingSelector = FindFirstObjectByType<BuildingSelector>();
        }

        if (panelController == null)
        {
            panelController = FindFirstObjectByType<PanelController>();
        }
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (itemPrefab != null && listRoot != null)
        {
            itemList = new ReusableUIList<BuildingListItem>(
                itemPrefab,
                listRoot
            );
        }
        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(
                HandleSearchChanged
            );
        }

        if (categoryDropdown != null)
        {
            categoryDropdown.onValueChanged.AddListener(
                HandleCategoryChanged
            );
        }
    }

    private void OnEnable()
    {
        if (productionSystem != null)
            productionSystem.BuildingsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
            productionSystem.BuildingsChanged -= Refresh;
    }

    private void OnDestroy()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(
                HandleSearchChanged
            );
        }

        if (categoryDropdown != null)
        {
            categoryDropdown.onValueChanged.RemoveListener(
                HandleCategoryChanged
            );
        }
    }

    private void Refresh()
    {
        if (productionSystem == null || itemList == null) return;

        int itemIndex = 0;

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null || building.Definition == null) continue;
            if (!MatchesFilter(building)) continue;
            BuildingListItem item = itemList.GetOrCreate(itemIndex);
            if (item == null) continue;

            item.Initialize(building, HandleBuildingSelected);
            itemIndex++;
        }

        itemList.HideFrom(itemIndex);
        if (emptyText != null) emptyText.SetActive(itemIndex == 0);
    }

    private void HandleBuildingSelected(
    BuildingInstance building)
    {
        if (building == null || buildingSelector == null)
        {
            return;
        }

        if (mapCamera != null)
        {
            mapCamera.FocusOn(
                building.WorldPosition
            );
        }

        buildingSelector.SelectBuilding(building);

        if (panelController != null)
        {
            panelController.CloseCurrentPanel();
        }
    }
    private bool MatchesFilter(BuildingInstance building)
    {
        if (building == null || building.Definition == null) return false;
        BuildingDefinition definition = building.Definition;
        int categoryIndex = categoryDropdown != null ? categoryDropdown.value : 0;

        bool matchesCategory = categoryIndex switch
        {
            0 => true,
            1 => definition.category ==
                 BuildingCategory.Resource,
            2 => definition.category ==
                 BuildingCategory.Processing,
            3 => definition.category ==
                 BuildingCategory.Commercial,
            4 => definition.category ==
                 BuildingCategory.Storage,
            _ => true
        };

        if (!matchesCategory) return false;

        string query = searchInput != null ? searchInput.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(query)) return true;

        bool matchesName =
            !string.IsNullOrEmpty(definition.displayName) &&
            definition.displayName.IndexOf(
                query,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;

        bool matchesDefinitionId =
            !string.IsNullOrEmpty(definition.buildingId) &&
            definition.buildingId.IndexOf(
                query,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;

        bool matchesInstanceId =
            !string.IsNullOrEmpty(building.InstanceId) &&
            building.InstanceId.IndexOf(
                query,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;

        return
            matchesName ||
            matchesDefinitionId ||
            matchesInstanceId;
    }

    private void HandleSearchChanged(string value)
    {
        Refresh();
    }

    private void HandleCategoryChanged(int value)
    {
        Refresh();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingCatalogPanel : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private GameDefinitionCatalog definitionCatalog;
    [SerializeField] private BuildingPlacementController buildingPlacement;

    [Header("List")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private BuildingCatalogItemUI itemPrefab;
    [SerializeField] private GameObject emptyText;

    [Header("Actions")]
    [SerializeField] private Button startPlacementButton;
    [SerializeField] private ManagementPanelController managementPanelController;

    private ReusableUIList<BuildingCatalogItemUI> itemList;

    private readonly List<BuildingDefinition> definitions =
        new List<BuildingDefinition>();

    private readonly Dictionary<
        BuildingDefinition,
        BuildingCatalogItemUI
    > itemsByDefinition =
        new Dictionary<BuildingDefinition, BuildingCatalogItemUI>();

    private void Awake()
    {
        if (definitionCatalog == null)
        {
            definitionCatalog =
                FindFirstObjectByType<GameDefinitionCatalog>();
        }

        if (buildingPlacement == null)
        {
            buildingPlacement =
                FindFirstObjectByType<BuildingPlacementController>();
        }

        if (managementPanelController == null)
        {
            managementPanelController =
                FindFirstObjectByType<ManagementPanelController>();
        }

        if (startPlacementButton != null)
        {
            startPlacementButton.onClick.AddListener(
                HandleStartPlacementClicked
            );
        }

        itemList =
            new ReusableUIList<BuildingCatalogItemUI>(
                itemPrefab,
                listRoot
            );

        if (definitionCatalog != null &&
            definitionCatalog.BuildingCount == 0)
        {
            definitionCatalog.Reload();
        }
    }

    private void OnEnable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.SelectedBuildingChanged +=
                HandleSelectedBuildingChanged;
        }

        BuildList();
    }

    private void OnDisable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.SelectedBuildingChanged -=
                HandleSelectedBuildingChanged;
        }
    }

    private void OnDestroy()
    {
        if (startPlacementButton != null)
        {
            startPlacementButton.onClick.RemoveListener(
                HandleStartPlacementClicked
            );
        }
    }



    private void BuildList()
    {
        definitions.Clear();
        itemsByDefinition.Clear();

        if (definitionCatalog == null || itemList == null)
        {
            SetEmptyState(true);
            return;
        }

        foreach (
            BuildingDefinition definition
            in definitionCatalog.BuildingDefinitions
        )
        {
            if (definition != null)
            {
                definitions.Add(definition);
            }
        }

        definitions.Sort(CompareDefinitions);

        int visibleItemCount = 0;

        foreach (BuildingDefinition definition in definitions)
        {
            BuildingCatalogItemUI item =
                itemList.GetOrCreate(visibleItemCount);

            if (item == null)
            {
                continue;
            }

            item.Initialize(
                definition,
                HandleBuildingSelected
            );

            itemsByDefinition[definition] = item;
            visibleItemCount++;
        }

        itemList.HideFrom(visibleItemCount);
        SetEmptyState(visibleItemCount == 0);

        HandleSelectedBuildingChanged(
            buildingPlacement != null
                ? buildingPlacement.SelectedBuilding
                : null
        );
    }

    private void HandleBuildingSelected(
        BuildingDefinition definition
    )
    {
        if (buildingPlacement == null)
        {
            return;
        }

        buildingPlacement.SelectBuilding(definition);
    }

    private void HandleSelectedBuildingChanged(
    BuildingDefinition selectedDefinition
)
    {
        foreach (
            KeyValuePair<
                BuildingDefinition,
                BuildingCatalogItemUI
            > pair in itemsByDefinition
        )
        {
            pair.Value.SetSelected(
                pair.Key == selectedDefinition
            );
        }

        if (startPlacementButton != null)
        {
            startPlacementButton.interactable =
                selectedDefinition != null &&
                selectedDefinition.isEnabled;
        }
    }

    private void HandleStartPlacementClicked()
    {
        if (buildingPlacement == null)
        {
            return;
        }

        BuildingDefinition selectedDefinition =
            buildingPlacement.SelectedBuilding;

        if (selectedDefinition == null ||
            !selectedDefinition.isEnabled)
        {
            return;
        }

        if (managementPanelController != null)
        {
            managementPanelController.CloseCurrentPanel();
        }

        buildingPlacement.EnterPlacementMode();
    }

    private void SetEmptyState(bool isEmpty)
    {
        if (emptyText != null)
        {
            emptyText.SetActive(isEmpty);
        }
    }

    private static int CompareDefinitions(
        BuildingDefinition left,
        BuildingDefinition right
    )
    {
        int categoryComparison =
            left.category.CompareTo(right.category);

        if (categoryComparison != 0)
        {
            return categoryComparison;
        }

        return string.Compare(
            left.displayName,
            right.displayName,
            StringComparison.Ordinal
        );
    }
}
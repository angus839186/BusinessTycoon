using System;
using System.Collections.Generic;

public partial class BuildingCatalogPanel
{
    private ReusableUIList<BuildingCatalogItem> itemList;

    private readonly List<BuildingDefinition> definitions =
        new List<BuildingDefinition>();

    private readonly Dictionary<
        BuildingDefinition, BuildingCatalogItem> itemsByDefinition =
        new Dictionary<BuildingDefinition, BuildingCatalogItem>();

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
            if (!MatchesFilter(definition))
            {
                continue;
            }
            BuildingCatalogItem item = itemList.GetOrCreate(visibleItemCount);

            if (item == null)
            {
                continue;
            }

            item.Initialize(definition, HandleBuildingSelected);

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

    private bool MatchesFilter(
    BuildingDefinition definition
)
    {
        if (definition == null)
        {
            return false;
        }

        int categoryIndex =
            categoryDropdown != null
                ? categoryDropdown.value
                : 0;

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

        if (!matchesCategory)
        {
            return false;
        }

        string query =
            searchInput != null
                ? searchInput.text.Trim()
                : string.Empty;

        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        bool matchesName =
            !string.IsNullOrEmpty(definition.displayName) &&
            definition.displayName.IndexOf(
                query,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;

        bool matchesId =
            !string.IsNullOrEmpty(definition.buildingId) &&
            definition.buildingId.IndexOf(
                query,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;

        return matchesName || matchesId;
    }

    private void HandleSearchChanged(string value)
    {
        BuildList();
    }

    private void HandleCategoryChanged(int value)
    {
        BuildList();
    }

    private void HandleSelectedBuildingChanged(
BuildingDefinition selectedDefinition)
    {
        foreach (
            KeyValuePair<
                BuildingDefinition,
                BuildingCatalogItem
            > pair in itemsByDefinition
        )
        {
            pair.Value.SetSelected(pair.Key == selectedDefinition);
        }
    }

    private void SetEmptyState(bool isEmpty)
    {
        if (emptyText != null)
        {
            emptyText.SetActive(isEmpty);
        }
    }

    private static int CompareDefinitions(BuildingDefinition left, BuildingDefinition right)
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
using System;

public sealed partial class BuildingListPanel
{
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
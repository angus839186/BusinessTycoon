using UnityEngine;

public sealed partial class ProductionSystem
{
    private void TickBuilding(BuildingInstance building, float deltaSeconds)
    {
        BuildingDefinition definition = building.Definition;

        if (definition == null)
            return;

        TickMaintenance(building, definition);

        if (
    definition.category != BuildingCategory.Resource &&
    definition.category != BuildingCategory.Processing &&
    definition.category != BuildingCategory.Commercial
)
            return;

        ResourceAmount[] inputResources = GetInputResources(definition);
        ResourceAmount[] outputResources = GetOutputResources(definition);

        if (
            definition.category != BuildingCategory.Commercial &&
            (outputResources == null || outputResources.Length == 0)
        )
            return;

        if (
    (definition.category == BuildingCategory.Processing ||
     definition.category == BuildingCategory.Commercial) &&
    !building.Inventory.HasEnough(inputResources)
)
        {
            if (gameClock != null)
                building.SetLastProcessGameMinutes(gameClock.TotalGameMinutes);

            return;
        }

        if (gameClock == null)
            return;

        double processIntervalMinutes = GetProcessIntervalMinutes(building);

        if (processIntervalMinutes <= 0)
            return;

        double elapsedProcessMinutes =
            gameClock.TotalGameMinutes - building.LastProcessGameMinutes;

        if (elapsedProcessMinutes < processIntervalMinutes)
            return;

        building.SetLastProcessGameMinutes(gameClock.TotalGameMinutes);

        if (
            (definition.category == BuildingCategory.Processing ||
             definition.category == BuildingCategory.Commercial) &&
            !building.Inventory.TryConsumeAll(inputResources)
        )
            return;

        if (definition.category == BuildingCategory.Commercial)
        {
            SellResources(building, definition);
            return;
        }

        ProduceResources(building, definition, outputResources);
    }
    private ResourceAmount[] GetInputResources(BuildingDefinition definition)
    {
        if (definition == null)
            return null;

        if (
            definition.category == BuildingCategory.Processing &&
            definition.producedResource != null &&
            definition.producedResource.recipe != null &&
            definition.producedResource.recipe.inputResources != null
        )
            return definition.producedResource.recipe.inputResources;

        return definition.inputResources;
    }

    private ResourceAmount[] GetOutputResources(BuildingDefinition definition)
    {
        if (definition == null)
            return null;

        if (
            (definition.category == BuildingCategory.Resource ||
             definition.category == BuildingCategory.Processing) &&
            definition.producedResource != null &&
            definition.producedResource.recipe != null &&
            definition.producedResource.recipe.outputResources != null &&
            definition.producedResource.recipe.outputResources.Length > 0
        )
            return definition.producedResource.recipe.outputResources;

        return definition.outputResources;
    }

    private double GetProcessIntervalMinutes(BuildingInstance building)
    {
        if (building == null || building.Definition == null)
            return 0;

        BuildingDefinition definition = building.Definition;
        double minutes = definition.processInterval.ToMinutes();

        if (minutes <= 0)
            return 0;

        float speed = Mathf.Max(0.01f, definition.workSpeedMultiplier);

        if (definition.category == BuildingCategory.Resource)
        {
            if (gameRules != null)
                speed *= gameRules.ResourceProductionSpeedMultiplier;

            LocationValueModifiers modifiers = GetLocationModifiers(building);
            speed *= modifiers.resourceProductionSpeedMultiplier;
        }

        return minutes / speed;
    }

    public float GetProcessProgress(BuildingInstance building)
    {
        if (gameClock == null || building == null || building.Definition == null)
            return 0f;

        if (IsWaitingForInput(building))
            return 0f;

        double intervalMinutes = GetProcessIntervalMinutes(building);

        if (intervalMinutes <= 0)
            return 0f;

        double elapsedMinutes = gameClock.TotalGameMinutes - building.LastProcessGameMinutes;
        return Mathf.Clamp01((float)(elapsedMinutes / intervalMinutes));
    }
    public bool IsWaitingForInput(BuildingInstance building)
    {
        if (building == null || building.Definition == null)
            return false;

        BuildingDefinition definition = building.Definition;

        if (
            definition.category != BuildingCategory.Processing &&
            definition.category != BuildingCategory.Commercial
        )
            return false;

        return !building.Inventory.HasEnough(GetInputResources(definition));
    }

    private void ProduceResources(
    BuildingInstance building,
    BuildingDefinition definition,
    ResourceAmount[] outputResources
)
    {
        bool changed = false;
        foreach (ResourceAmount output in outputResources)
        {
            if (output == null || output.resource == null || output.amount <= 0)
                continue;

            building.Inventory.Add(output.resource, output.amount);
            changed = true;

            if (logProduction)
            {
                Debug.Log(
                    $"Produced: Building={definition.displayName}, " +
                    $"Resource={output.resource.displayName}, " +
                    $"Amount={output.amount}, " +
                    $"Stored={building.Inventory.GetAmount(output.resource)}"
                );
            }
        }
        if (changed)
            MarkDataChanged();
    }
    // IsWaitingForInput
    // ProduceResources
}
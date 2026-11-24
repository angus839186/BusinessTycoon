public sealed partial class ProductionSystem
{
    private bool HasEnoughInput(
    BuildingInstance building,
    BuildingDefinition definition
)
    {
        if (building == null ||
            definition == null)
        {
            return false;
        }

        if (definition.category == BuildingCategory.Processing)
        {
            ResourceAmount[] inputResources =
                GetInputResources(definition);

            return
                inputResources != null &&
                inputResources.Length > 0 &&
                building.Inventory.HasEnough(
                    inputResources
                );
        }

        if (definition.category ==
            BuildingCategory.Commercial)
        {
            return
                definition.saleResource != null &&
                definition.saleAmount > 0 &&
                building.Inventory.GetAmount(
                    definition.saleResource
                ) >= definition.saleAmount;
        }

        return true;
    }

    private bool TryConsumeInput(
        BuildingInstance building,
        BuildingDefinition definition
    )
    {
        if (building == null ||
            definition == null)
        {
            return false;
        }

        if (definition.category == BuildingCategory.Processing)
        {
            ResourceAmount[] inputResources =
                GetInputResources(definition);

            return
                inputResources != null &&
                inputResources.Length > 0 &&
                building.Inventory.TryConsumeAll(
                    inputResources
                );
        }

        if (definition.category ==
            BuildingCategory.Commercial)
        {
            return building.Inventory.TryConsume(
                definition.saleResource,
                definition.saleAmount
            );
        }

        return true;
    }
    private ResourceAmount[] GetInputResources(
    BuildingDefinition definition
)
    {
        if (definition == null ||
            definition.category !=
                BuildingCategory.Processing ||
            definition.producedResource == null ||
            definition.producedResource.recipe == null)
        {
            return null;
        }

        return definition
            .producedResource
            .recipe
            .inputResources;
    }

    private ResourceAmount[] GetOutputResources(
    BuildingDefinition definition
)
    {
        if (definition == null ||
            (definition.category !=
            BuildingCategory.Resource &&
            definition.category !=
            BuildingCategory.Processing) ||
            definition.producedResource == null ||
            definition.producedResource.recipe == null)
        {
            return null;
        }

        return definition
            .producedResource
            .recipe
            .outputResources;
    }
}
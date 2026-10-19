public sealed partial class TransportSystem
{
    private bool NeedsResource(
    BuildingDefinition definition,
    ResourceDefinition resource
)
    {
        if (definition == null || resource == null)
            return false;

        if (definition.category == BuildingCategory.Storage)
            return definition.acceptsAnyResource;

        if (definition.category == BuildingCategory.Processing)
            return ContainsResource(GetProcessingInputResources(definition), resource);

        if (definition.category == BuildingCategory.Commercial)
        {
            return
                definition.saleAmount > 0 &&
                definition.saleResource == resource;
        }

        return false;
    }

    private ResourceAmount[] GetProcessingInputResources(BuildingDefinition definition)
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
    private bool ContainsResource(ResourceAmount[] resources, ResourceDefinition resource)
    {
        if (resources == null || resource == null)
            return false;

        foreach (ResourceAmount resourceAmount in resources)
        {
            if (resourceAmount == null || resourceAmount.resource == null)
                continue;

            if (resourceAmount.resource == resource)
                return true;
        }

        return false;
    }
}
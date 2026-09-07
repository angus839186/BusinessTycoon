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
            return ContainsResource(definition.inputResources, resource);

        return false;
    }

    private ResourceAmount[] GetProcessingInputResources(BuildingDefinition definition)
    {
        if (
            definition != null &&
            definition.producedResource != null &&
            definition.producedResource.recipe != null &&
            definition.producedResource.recipe.inputResources != null
        )
            return definition.producedResource.recipe.inputResources;

        return definition != null ? definition.inputResources : null;
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
public sealed partial class ProductionSystem
{
    private readonly BuildingInventory globalInventory = new BuildingInventory();
    public BuildingInventory GlobalInventory => globalInventory;

    private void InitializeStartingResources()
    {
        foreach (ResourceAmount resourceAmount in startingResources)
        {
            if (resourceAmount == null || resourceAmount.resource == null || resourceAmount.amount <= 0)
                continue;

            globalInventory.Add(resourceAmount.resource, resourceAmount.amount);
        }

        MarkDataChanged();
    }
}
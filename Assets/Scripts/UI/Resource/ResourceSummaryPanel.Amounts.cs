using System.Collections.Generic;

public sealed partial class ResourceSummaryPanel
{
    private readonly Dictionary<ResourceDefinition, int> totals = new Dictionary<ResourceDefinition, int>();

    private void RefreshAmounts()
    {
        totals.Clear();

        foreach (ResourceDefinition definition in items.Keys)
            totals.Add(definition, 0);

        if (productionSystem != null)
        {
            AddInventory(productionSystem.GlobalInventory);

            foreach (
                BuildingInstance building
                in productionSystem.Buildings
            )
            {
                if (building != null)
                    AddInventory(building.Inventory);
            }
        }

        foreach (
            KeyValuePair<ResourceDefinition, ResourceSummaryItem>
            pair in items
        )
        {
            int amount = totals.TryGetValue(
                pair.Key,
                out int total
            )
                ? total
                : 0;

            pair.Value.SetAmount(amount);
        }
    }

    private void AddInventory(BuildingInventory inventory)
    {
        if (inventory == null)
            return;

        foreach (ResourceStack stack in inventory.Resources)
        {
            if (
                stack == null ||
                stack.resource == null ||
                stack.amount <= 0
            )
            {
                continue;
            }

            if (!totals.ContainsKey(stack.resource))
                totals.Add(stack.resource, 0);

            totals[stack.resource] += stack.amount;
        }
    }
}
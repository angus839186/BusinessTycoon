using System.Collections.Generic;

public static class EconomyValueCalculator
{
    public static long CalculateTotalAssetValue(
        ProductionSystem productionSystem
    )
    {
        if (productionSystem == null)
            return 0;

        long total = CalculateInventoryValue(
            productionSystem.GlobalInventory
        );

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null)
                continue;

            total += CalculateAmountsValue(
                building.ConstructionCostSnapshot
            );

            total += CalculateInventoryValue(building.Inventory);
        }

        return total;
    }

    private static long CalculateInventoryValue(
        BuildingInventory inventory
    )
    {
        if (inventory == null)
            return 0;

        long total = 0;

        foreach (ResourceStack stack in inventory.Resources)
        {
            if (stack == null)
                continue;

            total += CalculateValue(stack.resource, stack.amount);
        }

        return total;
    }

    private static long CalculateAmountsValue(
        IReadOnlyList<ResourceAmount> amounts
    )
    {
        if (amounts == null)
            return 0;

        long total = 0;

        foreach (ResourceAmount amount in amounts)
        {
            if (amount == null)
                continue;

            total += CalculateValue(amount.resource, amount.amount);
        }

        return total;
    }

    private static long CalculateValue(
        ResourceDefinition resource,
        int amount
    )
    {
        if (resource == null || amount <= 0)
            return 0;

        return (long)System.Math.Max(0, resource.basePrice) * amount;
    }
}
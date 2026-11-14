using System.Collections.Generic;

public static class AssetValueCalculator
{
    public static long CalculateTotalAssetValue(
    ProductionSystem productionSystem, MarketSystem marketSystem)
    {
        if (productionSystem == null)
            return 0;

        long total = CalculateInventoryValue(
                    productionSystem.GlobalInventory,
                    marketSystem
                );

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null)
                continue;

            total += CalculateAmountsValue(
                building.ConstructionCostSnapshot,
                marketSystem
            );

            total += CalculateInventoryValue(
                building.Inventory,
                marketSystem
            );
        }

        return total;
    }

    private static long CalculateInventoryValue(
    BuildingInventory inventory,
    MarketSystem marketSystem
)
    {
        if (inventory == null)
            return 0;

        long total = 0;

        foreach (ResourceStack stack in inventory.Resources)
        {
            if (stack == null)
                continue;

            total += CalculateValue(
                stack.resource,
                stack.amount,
                marketSystem);
        }

        return total;
    }

    private static long CalculateAmountsValue(
    IReadOnlyList<ResourceAmount> amounts,
    MarketSystem marketSystem
)
    {
        if (amounts == null)
            return 0;

        long total = 0;

        foreach (ResourceAmount amount in amounts)
        {
            if (amount == null)
                continue;

            total += CalculateValue(
                amount.resource,
                amount.amount,
                marketSystem
            );
        }

        return total;
    }

    private static long CalculateValue(
    ResourceDefinition resource,
    int amount,
    MarketSystem marketSystem
)
    {
        if (resource == null || amount <= 0)
            return 0;

        int unitPrice =
    marketSystem != null
        ? marketSystem.GetUnitPrice(resource)
        : System.Math.Max(0, resource.basePrice);

        return (long)unitPrice * amount;
    }
}
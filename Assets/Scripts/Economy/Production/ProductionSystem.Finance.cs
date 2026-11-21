using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class ProductionSystem
{
    private void TickMaintenance(BuildingInstance building, BuildingDefinition definition)
    {
        if (definition.maintenanceCosts == null || definition.maintenanceCosts.Length == 0)
            return;

        double intervalMinutes = GetMaintenanceIntervalMinutes();

        if (intervalMinutes <= 0)
            return;

        double elapsedMinutes =
            gameClock.TotalGameMinutes - building.LastMaintenanceGameMinutes;

        if (elapsedMinutes < intervalMinutes)
            return;

        float multiplier = gameRules.MaintenanceCostMultiplier;
        LocationValueModifiers modifiers = GetLocationModifiers(building);
        multiplier *= modifiers.maintenanceCostMultiplier;
        ResourceAmount[] maintenanceCosts = CalculateModifiedCosts(definition.maintenanceCosts, multiplier);

        if (TryPay(maintenanceCosts, definition.displayName, "Maintenance"))
            building.SetLastMaintenanceGameMinutes(gameClock.TotalGameMinutes);
    }
    private double GetMaintenanceIntervalMinutes() => gameRules.MaintenanceIntervalMinutes;

    public bool TryPayConstructionCost(
    BuildingDefinition definition,
    Vector3 worldPosition,
    double longitude,
    double latitude,
    out ResourceAmount[] paidCosts
)
    {
        paidCosts = Array.Empty<ResourceAmount>();

        if (definition == null)
            return false;

        float multiplier = GetConstructionCostMultiplier(worldPosition, longitude, latitude);

        paidCosts = CalculateModifiedCosts(
            definition.constructionCosts,
            multiplier
        );

        return TryPay(
            paidCosts,
            definition.displayName,
            "Construction"
        );
    }
    public bool TryPayDemolitionCost(BuildingInstance building, float demolitionCostRate)
    {
        if (building == null)
            return false;

        List<ResourceAmount> demolitionCosts = new List<ResourceAmount>();

        foreach (ResourceAmount cost in building.ConstructionCostSnapshot)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            int amount = Mathf.CeilToInt(cost.amount * demolitionCostRate);

            if (amount <= 0)
                continue;

            demolitionCosts.Add(new ResourceAmount
            {
                resource = cost.resource,
                amount = amount
            });
        }

        return TryPay(
            demolitionCosts.ToArray(),
            building.Definition != null ? building.Definition.displayName : building.InstanceId,
            "Demolition"
        );
    }
    public bool TryPay(
    ResourceAmount[] costs,
    string sourceName,
    string expenseType
)
    {
        if (costs == null || costs.Length == 0)
            return true;

        if (!globalInventory.HasEnough(costs))
            return false;

        int expense = CalculateExpense(costs);

        globalInventory.TryConsumeAll(costs);

        AddExpenseRecord(
            sourceName,
            expenseType,
            costs,
            expense
        );

        GetOrCreateMonthlyFinanceRecord().AddExpense(expense);

        MarkFinanceChanged();
        return true;
    }
    private void AddExpenseRecord(
    string sourceName,
    string expenseType,
    ResourceAmount[] paidResources,
    int expense
)
    {
        ExpenseRecord record = new ExpenseRecord(
            sourceName,
            expenseType,
            FormatResources(paidResources),
            expense,
            globalInventory.GetAmount(moneyResource), gameClock.TotalGameMinutes
        );

        expenseRecords.Insert(0, record);

        TrimRecords(expenseRecords, maxExpenseRecords);
    }

    private void SellResources(BuildingInstance building, BuildingDefinition definition)
    {
        if (definition.saleResource == null || definition.saleAmount <= 0)
            return;

        ResourceDefinition soldResource =
            definition.saleResource;

        int soldAmount =
            definition.saleAmount;

        int income = CalculateSaleIncome(
            building,
            soldResource,
            soldAmount
        );

        if (income <= 0)
        {
            return;
        }

        globalInventory.Add(
            moneyResource,
            income
        );

        string soldResources =
            $"{soldResource.displayName} x{soldAmount}";

        IncomeRecord record = new IncomeRecord(
            definition.displayName,
            soldResources,
            income,
            globalInventory.GetAmount(moneyResource),
            gameClock.TotalGameMinutes
        );

        incomeRecords.Insert(0, record);

        TrimRecords(incomeRecords, maxIncomeRecords);

        GetOrCreateMonthlyFinanceRecord().AddIncome(income);

        MarkFinanceChanged();

        if (logProduction)
        {
            Debug.Log(
                $"Sold: Building={definition.displayName}, " +
                $"Resources={record.soldResources}, " +
                $"Income={income}, " +
                $"Money={globalInventory.GetAmount(moneyResource)}"
            );
        }
    }

    private int CalculateSaleIncome(
    BuildingInstance building,
    ResourceDefinition resource,
    int amount
)
    {
        if (resource == null ||
            amount <= 0)
        {
            return 0;
        }

        int unitPrice = marketSystem != null
        ? marketSystem.GetUnitPrice(resource)
        : Mathf.Max(0, resource.basePrice);

        int baseIncome =
            unitPrice * amount;

        float multiplier = gameRules.CommercialSalePriceMultiplier;

        LocationValueModifiers modifiers =
            GetLocationModifiers(building);

        multiplier *=
            modifiers.commercialSalePriceMultiplier;

        return Mathf.RoundToInt(
            baseIncome * multiplier
        );
    }

    private ResourceAmount[] CalculateModifiedCosts(ResourceAmount[] baseCosts, float multiplier)
    {
        if (baseCosts == null || baseCosts.Length == 0)
            return Array.Empty<ResourceAmount>();

        List<ResourceAmount> result = new List<ResourceAmount>();

        foreach (ResourceAmount cost in baseCosts)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            result.Add(new ResourceAmount
            {
                resource = cost.resource,
                amount = Mathf.CeilToInt(cost.amount * multiplier)
            });
        }

        return result.ToArray();
    }

    private int CalculateExpense(ResourceAmount[] resources)
    {
        if (resources == null)
            return 0;

        int total = 0;

        foreach (ResourceAmount resourceAmount in resources)
        {
            if (
                resourceAmount == null ||
                resourceAmount.resource == null ||
                resourceAmount.amount <= 0
            )
                continue;

            if (resourceAmount.resource == moneyResource)
            {
                total += resourceAmount.amount;
            }
            else
            {
                int unitPrice =
                    marketSystem != null
                        ? marketSystem.GetUnitPrice(resourceAmount.resource)
                        : Mathf.Max(
                            0,
                            resourceAmount.resource.basePrice
                        );

                total += unitPrice * resourceAmount.amount;
            }
        }

        return total;
    }

    private string FormatResources(ResourceAmount[] resources)
    {
        if (resources == null || resources.Length == 0)
            return "";

        List<string> parts = new List<string>();

        foreach (ResourceAmount resourceAmount in resources)
        {
            if (
                resourceAmount == null ||
                resourceAmount.resource == null ||
                resourceAmount.amount <= 0
            )
                continue;

            parts.Add($"{resourceAmount.resource.displayName} x{resourceAmount.amount}");
        }

        return string.Join(", ", parts);
    }

    private static void TrimRecords<T>(List<T> records, int maxCount)
    {
        int limit = Math.Max(0, maxCount);

        if (records.Count > limit)
            records.RemoveRange(limit, records.Count - limit);
    }
}
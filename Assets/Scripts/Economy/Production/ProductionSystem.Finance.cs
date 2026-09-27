using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class ProductionSystem
{
    private void TickMaintenance(BuildingInstance building, BuildingDefinition definition)
    {
        if (
            gameClock == null ||
            definition == null ||
            definition.maintenanceCosts == null ||
            definition.maintenanceCosts.Length == 0
        )
            return;

        double intervalMinutes = GetMaintenanceIntervalMinutes();

        if (intervalMinutes <= 0)
            return;

        double elapsedMinutes =
            gameClock.TotalGameMinutes - building.LastMaintenanceGameMinutes;

        if (elapsedMinutes < intervalMinutes)
            return;

        float multiplier = gameRules != null ? gameRules.MaintenanceCostMultiplier : 1f;
        LocationValueModifiers modifiers = GetLocationModifiers(building);
        multiplier *= modifiers.maintenanceCostMultiplier;
        ResourceAmount[] maintenanceCosts = CalculateModifiedCosts(definition.maintenanceCosts, multiplier);

        if (TryPay(maintenanceCosts, definition.displayName, "Maintenance"))
            building.SetLastMaintenanceGameMinutes(gameClock.TotalGameMinutes);
    }
    private double GetMaintenanceIntervalMinutes()
    {
        if (gameRules == null || gameRules.Config == null)
            return 0;

        return gameRules.Config.buildingMaintenanceInterval.ToMinutes();
    }

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

        float multiplier = GetConstructionCostMultiplier(
            definition,
            worldPosition,
            longitude,
            latitude
        );

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
            moneyResource != null ? globalInventory.GetAmount(moneyResource) : 0,
            gameClock != null ? gameClock.TotalGameMinutes : 0
        );

        expenseRecords.Insert(0, record);

        while (expenseRecords.Count > maxExpenseRecords)
            expenseRecords.RemoveAt(expenseRecords.Count - 1);
    }

    private void SellResources(
    BuildingInstance building,
    BuildingDefinition definition
)
    {
        if (moneyResource == null)
            return;

        int income = CalculateSaleIncome(building, definition.inputResources);

        if (income <= 0)
            return;

        globalInventory.Add(moneyResource, income);

        IncomeRecord record = new IncomeRecord(
            definition.displayName,
            FormatResources(definition.inputResources),
            income,
            globalInventory.GetAmount(moneyResource),
            gameClock != null ? gameClock.TotalGameMinutes : 0
        );

        incomeRecords.Insert(0, record);

        while (incomeRecords.Count > maxIncomeRecords)
            incomeRecords.RemoveAt(incomeRecords.Count - 1);
        GetOrCreateMonthlyFinanceRecord().AddIncome(income);
        MarkFinanceChanged();

        if (logProduction)
        {
            Debug.Log(
                $"Sold: Building={definition.displayName}, " +
                $"Resources={record.soldResources}, " +
                $"Income={income}, Money={globalInventory.GetAmount(moneyResource)}"
            );
        }
    }

    private int CalculateSaleIncome(
    BuildingInstance building,
    ResourceAmount[] resources
)
    {
        if (resources == null)
            return 0;

        int total = 0;

        foreach (ResourceAmount resourceAmount in resources)
        {
            if (resourceAmount == null || resourceAmount.resource == null || resourceAmount.amount <= 0)
                continue;

            total += resourceAmount.resource.basePrice * resourceAmount.amount;
        }

        float multiplier = gameRules != null ? gameRules.CommercialSalePriceMultiplier : 1f;
        LocationValueModifiers modifiers = GetLocationModifiers(building);
        multiplier *= modifiers.commercialSalePriceMultiplier;

        return Mathf.RoundToInt(total * multiplier);
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
                total += resourceAmount.amount;
            else
                total += resourceAmount.resource.basePrice * resourceAmount.amount;
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
}
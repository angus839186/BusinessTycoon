using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class ProductionSystem
{
    private readonly List<MonthlyFinanceRecord> monthlyFinanceRecords = new List<MonthlyFinanceRecord>();
    public IReadOnlyList<MonthlyFinanceRecord> MonthlyFinanceRecords => monthlyFinanceRecords;

    private readonly List<IncomeRecord> incomeRecords = new List<IncomeRecord>();
    public IReadOnlyList<IncomeRecord> IncomeRecords => incomeRecords;

    private readonly List<ExpenseRecord> expenseRecords = new List<ExpenseRecord>();
    public IReadOnlyList<ExpenseRecord> ExpenseRecords => expenseRecords;

    private void InitializeFinance()
    {
        GetOrCreateMonthlyFinanceRecord();
        gameClock.MonthChanged += HandleMonthChanged;
    }

    private void ShutdownFinance()
    {
        if (gameClock != null)
            gameClock.MonthChanged -= HandleMonthChanged;
    }
    private void HandleMonthChanged()
    {
        GetOrCreateMonthlyFinanceRecord();
        MarkFinanceChanged();
    }
    private MonthlyFinanceRecord GetOrCreateMonthlyFinanceRecord()
    {
        int currentMonth = gameClock.MonthIndex + 1;

        foreach (MonthlyFinanceRecord record in monthlyFinanceRecords)
        {
            if (record.month == currentMonth)
                return record;
        }

        MonthlyFinanceRecord newRecord = new MonthlyFinanceRecord(currentMonth);

        monthlyFinanceRecords.Add(newRecord);
        return newRecord;
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

    private static void TrimRecords<T>(List<T> records, int maxCount)
    {
        int limit = Math.Max(0, maxCount);

        if (records.Count > limit)
            records.RemoveRange(limit, records.Count - limit);
    }
}
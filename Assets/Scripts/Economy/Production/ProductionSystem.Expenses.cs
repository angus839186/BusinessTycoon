using System.Collections.Generic;
using UnityEngine;

public sealed partial class ProductionSystem
{
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
}
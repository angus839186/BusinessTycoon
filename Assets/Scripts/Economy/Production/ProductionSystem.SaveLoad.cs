using UnityEngine;

public sealed partial class ProductionSystem
{
    private bool isRestoringState;

    public void PrepareForLoad()
    {
        isRestoringState = true;

        foreach (BuildingInstance building in buildings)
        {
            if (building != null)
                Destroy(building.gameObject);
        }

        buildings.Clear();
        globalInventory.Clear();
        incomeRecords.Clear();
        expenseRecords.Clear();
    }

    public void RestoreGlobalResource(
        ResourceDefinition resource,
        int amount
    )
    {
        if (resource == null || amount <= 0)
            return;

        globalInventory.Add(resource, amount);
    }

    public void RestoreIncomeRecord(IncomeRecord record)
    {
        if (record == null)
            return;

        incomeRecords.Add(record);

        while (incomeRecords.Count > maxIncomeRecords)
            incomeRecords.RemoveAt(incomeRecords.Count - 1);
    }

    public void RestoreExpenseRecord(ExpenseRecord record)
    {
        if (record == null)
            return;

        expenseRecords.Add(record);

        while (expenseRecords.Count > maxExpenseRecords)
            expenseRecords.RemoveAt(expenseRecords.Count - 1);
    }

    public void CompleteLoad()
    {
        isRestoringState = false;

        FinanceChanged?.Invoke();
        DataChanged?.Invoke();
    }
}
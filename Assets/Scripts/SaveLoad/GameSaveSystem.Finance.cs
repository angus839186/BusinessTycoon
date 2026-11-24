public sealed partial class GameSaveSystem
{
    private void RestoreFinance(GameSaveData saveData)
    {
        if (saveData.incomeRecords != null)
        {
            foreach (IncomeRecordSaveData record in saveData.incomeRecords)
            {
                if (record == null)
                    continue;

                productionSystem.RestoreIncomeRecord(
                    new IncomeRecord(
                        record.buildingName,
                        record.soldResources,
                        record.income,
                        record.moneyAfterSale,
                        record.gameMinutes
                    )
                );
            }
        }

        if (saveData.expenseRecords != null)
        {
            foreach (ExpenseRecordSaveData record in saveData.expenseRecords)
            {
                if (record == null)
                    continue;

                productionSystem.RestoreExpenseRecord(
                    new ExpenseRecord(
                        record.sourceName,
                        record.expenseType,
                        record.paidResources,
                        record.expense,
                        record.moneyAfterExpense,
                        record.gameMinutes
                    )
                );
            }
        }
        if (saveData.monthlyFinanceRecords != null)
        {
            foreach (
                MonthlyFinanceRecordSaveData record
                in saveData.monthlyFinanceRecords
            )
            {
                if (record == null)
                    continue;

                productionSystem.RestoreMonthlyFinanceRecord(
                    record.month,
                    record.income,
                    record.expense
                );
            }
        }
    }
}
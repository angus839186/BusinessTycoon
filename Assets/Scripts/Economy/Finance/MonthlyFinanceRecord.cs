using System;

[Serializable]
public sealed class MonthlyFinanceRecord
{
    public int month;
    public int income;
    public int expense;

    public int Net => income - expense;

    public MonthlyFinanceRecord(int month)
    {
        this.month = month;
    }

    public void AddIncome(int amount)
    {
        income += Math.Max(0, amount);
    }

    public void AddExpense(int amount)
    {
        expense += Math.Max(0, amount);
    }
}
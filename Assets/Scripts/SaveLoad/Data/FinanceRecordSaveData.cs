using System;

[Serializable]
public sealed class IncomeRecordSaveData
{
    public string buildingName;
    public string soldResources;
    public int income;
    public int moneyAfterSale;
    public double gameMinutes;
}

[Serializable]
public sealed class ExpenseRecordSaveData
{
    public string sourceName;
    public string expenseType;
    public string paidResources;
    public int expense;
    public int moneyAfterExpense;
    public double gameMinutes;
}
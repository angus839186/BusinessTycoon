using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameSaveData
{
    public int saveVersion = 1;

    public double totalGameMinutes;
    public float speedMultiplier = 1f;

    public int nextBuildingInstanceIndex = 1;
    public int nextRouteIndex = 1;
    public double lastTransportGameMinutes;

    public List<ResourceStackSaveData> globalResources =
        new List<ResourceStackSaveData>();

    public List<BuildingSaveData> buildings =
        new List<BuildingSaveData>();

    public List<TransportRouteSaveData> transportRoutes =
        new List<TransportRouteSaveData>();

    public List<IncomeRecordSaveData> incomeRecords =
        new List<IncomeRecordSaveData>();

    public List<ExpenseRecordSaveData> expenseRecords =
        new List<ExpenseRecordSaveData>();
    public List<MonthlyFinanceRecordSaveData> monthlyFinanceRecords =
new List<MonthlyFinanceRecordSaveData>();
}
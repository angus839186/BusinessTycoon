using System;

[Serializable]
public sealed class GameRulesConfig
{
    public GameTimeInterval buildingMaintenanceInterval;
    public float demolitionCostRate = 0.2f;

    public float constructionCostMultiplier = 1f;
    public float maintenanceCostMultiplier = 1f;
    public float commercialSalePriceMultiplier = 1f;
    public float resourceProductionSpeedMultiplier = 1f;
    public float transportIntervalMultiplier = 1f;
}
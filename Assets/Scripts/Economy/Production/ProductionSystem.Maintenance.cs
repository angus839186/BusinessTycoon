public sealed partial class ProductionSystem
{
    private void TickMaintenance(BuildingInstance building, BuildingDefinition definition)
    {
        if (definition.maintenanceCosts == null || definition.maintenanceCosts.Length == 0)
            return;

        double intervalMinutes = GetMaintenanceIntervalMinutes();

        if (intervalMinutes <= 0)
            return;

        double elapsedMinutes = gameClock.TotalGameMinutes - building.LastMaintenanceGameMinutes;

        if (elapsedMinutes < intervalMinutes)
            return;

        float multiplier = gameRules.MaintenanceCostMultiplier;
        LocationValueModifiers modifiers = GetLocationModifiers(building);
        multiplier *= modifiers.maintenanceCostMultiplier;
        ResourceAmount[] maintenanceCosts = CalculateModifiedCosts(definition.maintenanceCosts, multiplier);

        if (TryPay(maintenanceCosts, definition.displayName, "Maintenance"))
            building.SetLastMaintenanceGameMinutes(gameClock.TotalGameMinutes);
    }

    private double GetMaintenanceIntervalMinutes()
    {
        return gameRules.MaintenanceIntervalMinutes;
    }
}
public sealed partial class ProductionSystem
{
    private void MarkDataChanged()
    {
        if (isRestoringState)
            return;

        DataChanged?.Invoke();
    }

    private void MarkFinanceChanged()
    {
        if (isRestoringState)
            return;

        FinanceChanged?.Invoke();
        MarkDataChanged();
    }

    public void Register(BuildingInstance building)
    {
        if (building == null || buildings.Contains(building))
            return;

        if (gameClock != null && building.LastMaintenanceGameMinutes <= 0)
            building.SetLastMaintenanceGameMinutes(gameClock.TotalGameMinutes);
        if (gameClock != null && building.LastProcessGameMinutes <= 0)
            building.SetLastProcessGameMinutes(gameClock.TotalGameMinutes);

        buildings.Add(building);
        MarkDataChanged();
    }

    public void Unregister(BuildingInstance building)
    {
        if (building == null)
            return;

        buildings.Remove(building);
        MarkDataChanged();
    }
}
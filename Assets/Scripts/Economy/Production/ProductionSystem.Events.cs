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
    private void MarkBuildingsChanged()
    {
        if (isRestoringState)
            return;

        BuildingsChanged?.Invoke();
        MarkDataChanged();
    }

    public void Register(BuildingInstance building)
    {
        if (building == null || buildings.Contains(building))
            return;

        double currentGameMinutes = gameClock.TotalGameMinutes;

        if (building.LastMaintenanceGameMinutes <= 0)
            building.SetLastMaintenanceGameMinutes(currentGameMinutes);

        if (building.LastProcessGameMinutes <= 0)
            building.SetLastProcessGameMinutes(currentGameMinutes);

        buildings.Add(building);
        MarkBuildingsChanged();
    }

    public void Unregister(BuildingInstance building)
    {
        if (building == null || !buildings.Remove(building))
            return;

        MarkBuildingsChanged();
    }

    public bool SetBuildingOperational(BuildingInstance building, bool operational)
    {
        if (building == null || !buildings.Contains(building))
            return false;

        if (!building.SetOperational(operational))
            return false;

        if (operational)
        {
            double currentGameMinutes = gameClock.TotalGameMinutes;
            building.SetLastProcessGameMinutes(currentGameMinutes);
            building.SetLastMaintenanceGameMinutes(currentGameMinutes);
        }

        MarkBuildingsChanged();
        return true;
    }
}
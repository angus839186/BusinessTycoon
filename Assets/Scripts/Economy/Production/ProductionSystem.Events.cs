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

        if (gameClock != null && building.LastMaintenanceGameMinutes <= 0)
            building.SetLastMaintenanceGameMinutes(gameClock.TotalGameMinutes);
        if (gameClock != null && building.LastProcessGameMinutes <= 0)
            building.SetLastProcessGameMinutes(gameClock.TotalGameMinutes);

        buildings.Add(building);
        MarkBuildingsChanged();
    }

    public void Unregister(BuildingInstance building)
    {
        if (building == null)
            return;

        buildings.Remove(building);
        MarkBuildingsChanged();
    }

    public bool SetBuildingOperational(
    BuildingInstance building,
    bool operational
)
    {
        if (building == null ||
            !buildings.Contains(building))
        {
            return false;
        }

        if (!building.SetOperational(operational))
        {
            return false;
        }

        if (operational &&
            gameClock != null)
        {
            double currentGameMinutes =
                gameClock.TotalGameMinutes;

            building.SetLastProcessGameMinutes(
                currentGameMinutes
            );

            building.SetLastMaintenanceGameMinutes(
                currentGameMinutes
            );
        }

        MarkBuildingsChanged();

        return true;
    }
}
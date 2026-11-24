using System.Collections.Generic;
using UnityEngine;

public sealed partial class ProductionSystem
{
    private readonly List<BuildingInstance> buildings = new List<BuildingInstance>();
    public IReadOnlyList<BuildingInstance> Buildings => buildings;

    private void RegisterExistingBuildings()
    {
        BuildingInstance[] existingBuildings = FindObjectsByType<BuildingInstance>(FindObjectsSortMode.None);

        foreach (BuildingInstance building in existingBuildings)
            Register(building);
    }

    private void TickBuildings()
    {
        for (int i = buildings.Count - 1; i >= 0; i--)
        {
            BuildingInstance building = buildings[i];

            if (building == null)
            {
                buildings.RemoveAt(i);
                continue;
            }

            if (!building.IsOperational)
                continue;

            TickBuilding(building);
        }
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
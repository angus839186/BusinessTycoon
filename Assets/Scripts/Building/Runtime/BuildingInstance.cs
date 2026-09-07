using UnityEngine;
using System.Collections.Generic;

public sealed class BuildingInstance : MonoBehaviour
{
    [SerializeField] private BuildingDefinition definition;
    [SerializeField] private string instanceId;

    [SerializeField] private Vector3 worldPosition;
    [SerializeField] private double longitude;
    [SerializeField] private double latitude;

    public Vector3 WorldPosition => worldPosition;
    public double Longitude => longitude;
    public double Latitude => latitude;

    private readonly List<ResourceAmount> constructionCostSnapshot =
    new List<ResourceAmount>();

    public IReadOnlyList<ResourceAmount> ConstructionCostSnapshot =>
        constructionCostSnapshot;

    public string InstanceId => instanceId;
    private readonly BuildingInventory inventory = new BuildingInventory();

    private double lastMaintenanceGameMinutes;

    public double LastMaintenanceGameMinutes => lastMaintenanceGameMinutes;
    private double lastProcessGameMinutes;

    public double LastProcessGameMinutes => lastProcessGameMinutes;

    public BuildingDefinition Definition => definition;
    public BuildingInventory Inventory => inventory;


    public void Initialize(BuildingDefinition buildingDefinition, string newInstanceId)
    {
        definition = buildingDefinition;
        instanceId = newInstanceId;
    }
    public void SetLocation(
    Vector3 newWorldPosition,
    double newLongitude,
    double newLatitude
)
    {
        worldPosition = newWorldPosition;
        longitude = newLongitude;
        latitude = newLatitude;
    }

    public void SetLastMaintenanceGameMinutes(double gameMinutes)
    {
        lastMaintenanceGameMinutes = gameMinutes;
    }
    public void SetLastProcessGameMinutes(double gameMinutes)
    {
        lastProcessGameMinutes = gameMinutes;
    }
    public void SetConstructionCostSnapshot(ResourceAmount[] costs)
    {
        constructionCostSnapshot.Clear();

        if (costs == null)
            return;

        foreach (ResourceAmount cost in costs)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            constructionCostSnapshot.Add(new ResourceAmount
            {
                resource = cost.resource,
                amount = cost.amount
            });
        }
    }
}
using UnityEngine;

public sealed partial class BuildingPlacementController
{
    public void PrepareForLoad(int nextInstanceIndex)
    {
        nextBuildingInstanceIndex = Mathf.Max(1, nextInstanceIndex);
        placedBuildings.Clear();
        isPlacementMode = false;
    }

    public BuildingInstance RestoreBuilding(
        BuildingDefinition definition,
        string instanceId,
        Vector3 worldPosition,
        double longitude,
        double latitude,
        ResourceAmount[] constructionCosts,
        double lastMaintenanceGameMinutes,
        double lastProcessGameMinutes
    )
    {
        if (
            definition == null ||
            string.IsNullOrWhiteSpace(instanceId)
        )
        {
            return null;
        }

        BuildingInstance instance = CreateBuildingInstance(
            definition,
            instanceId,
            worldPosition,
            longitude,
            latitude,
            constructionCosts
        );

        instance.SetLastMaintenanceGameMinutes(
            lastMaintenanceGameMinutes
        );

        instance.SetLastProcessGameMinutes(
            lastProcessGameMinutes
        );

        return instance;
    }
}
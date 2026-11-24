using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class GameSaveSystem
{
    private HashSet<string> RestoreBuildings(
        List<BuildingSaveData> savedBuildings
    )
    {
        HashSet<string> restoredIds = new HashSet<string>(
            StringComparer.Ordinal
        );

        if (savedBuildings == null)
            return restoredIds;

        foreach (BuildingSaveData savedBuilding in savedBuildings)
        {
            if (
                savedBuilding == null ||
                string.IsNullOrWhiteSpace(savedBuilding.instanceId)
            )
            {
                continue;
            }

            if (!restoredIds.Add(savedBuilding.instanceId))
            {
                Debug.LogWarning(
                    $"Duplicate building instanceId skipped: " +
                    $"{savedBuilding.instanceId}"
                );
                continue;
            }

            BuildingDefinition definition =
                definitionCatalog.FindBuilding(savedBuilding.buildingId);

            if (definition == null)
            {
                restoredIds.Remove(savedBuilding.instanceId);

                Debug.LogWarning(
                    $"Building definition not found: " +
                    $"{savedBuilding.buildingId}"
                );
                continue;
            }

            ResourceAmount[] constructionCosts =
                CreateResourceAmounts(
                    savedBuilding.constructionCostSnapshot
                );

            BuildingInstance instance =
                buildingPlacement.RestoreBuilding(
                    definition,
                    savedBuilding.instanceId,
                    savedBuilding.worldPosition,
                    savedBuilding.longitude,
                    savedBuilding.latitude,
                    constructionCosts,
                    savedBuilding.isOperational,
                    savedBuilding.lastMaintenanceGameMinutes,
                    savedBuilding.lastProcessGameMinutes
                );

            if (instance == null)
            {
                restoredIds.Remove(savedBuilding.instanceId);
                continue;
            }

            RestoreInventory(
                instance.Inventory,
                savedBuilding.inventory
            );
        }

        return restoredIds;
    }

    private void RestoreInventory(
        BuildingInventory inventory,
        List<ResourceStackSaveData> savedResources
    )
    {
        if (inventory == null || savedResources == null)
            return;

        inventory.Clear();

        foreach (ResourceStackSaveData savedResource in savedResources)
        {
            ResourceDefinition resource = ResolveResource(savedResource);

            if (resource != null)
                inventory.Add(resource, savedResource.amount);
        }
    }
    private ResourceAmount[] CreateResourceAmounts(
        List<ResourceStackSaveData> savedResources
    )
    {
        if (savedResources == null)
            return Array.Empty<ResourceAmount>();

        List<ResourceAmount> result = new List<ResourceAmount>();

        foreach (ResourceStackSaveData savedResource in savedResources)
        {
            ResourceDefinition resource = ResolveResource(savedResource);

            if (resource == null)
                continue;

            result.Add(new ResourceAmount
            {
                resource = resource,
                amount = savedResource.amount
            });
        }

        return result.ToArray();
    }
}
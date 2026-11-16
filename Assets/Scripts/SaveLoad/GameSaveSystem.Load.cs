using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public sealed partial class GameSaveSystem
{
    private const int CurrentSaveVersion = 2;

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        TryLoadGame();
    }

    public bool TryLoadGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Load Game can only run in Play Mode.");
            return false;
        }
        string loadPath = LoadPath;

        if (!File.Exists(loadPath))
        {
            Debug.LogWarning(
                $"Save file not found: {loadPath}"
            );

            return false;
        }

        if (
            productionSystem == null ||
            transportSystem == null ||
            gameClock == null ||
            buildingPlacement == null ||
            definitionCatalog == null
        )
        {
            Debug.LogError("GameSaveSystem has missing references.");
            return false;
        }

        GameSaveData saveData;

        try
        {
            string json = File.ReadAllText(loadPath, Encoding.UTF8);
            saveData = JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to read save file: {exception.Message}");
            return false;
        }

        if (saveData == null)
        {
            Debug.LogError("Save data is empty.");
            return false;
        }

        if (!TryMigrateSaveData(saveData))
        {
            Debug.LogError(
                $"Unsupported save version: {saveData.saveVersion}"
            );
            return false;
        }

        definitionCatalog.Reload();
        productionSystem.PrepareForLoad();

        bool loadSucceeded = false;

        try
        {
            RestoreGameState(saveData);
            loadSucceeded = true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to restore save: {exception}");
        }
        finally
        {
            productionSystem.CompleteLoad();
        }

        if (!loadSucceeded)
            return false;

        Debug.Log(
        $"Game loaded: {loadPath}, " +
        $"Buildings={saveData.buildings.Count}, " +
        $"Routes={saveData.transportRoutes.Count}"
        );

        return true;
    }

    private static bool TryMigrateSaveData(GameSaveData saveData)
    {
        if (saveData.saveVersion == 1)
        {
            if (saveData.buildings != null)
            {
                foreach (BuildingSaveData building in saveData.buildings)
                {
                    if (building != null)
                        building.isOperational = true;
                }
            }

            saveData.saveVersion = 2;
        }

        return saveData.saveVersion == CurrentSaveVersion;
    }

    public static bool CanLoadVersion(int version)
    {
        return
            version == 1 ||
            version == CurrentSaveVersion;
    }

    private void RestoreGameState(GameSaveData saveData)
    {
        gameClock.RestoreState(
            saveData.totalGameMinutes,
            saveData.speedMultiplier
        );

        buildingPlacement.PrepareForLoad(
            saveData.nextBuildingInstanceIndex
        );

        RestoreGlobalResources(saveData.globalResources);

        HashSet<string> restoredBuildingIds =
            RestoreBuildings(saveData.buildings);

        RestoreFinance(saveData);
        RestoreRoutes(saveData, restoredBuildingIds);
    }

    private void RestoreGlobalResources(
        List<ResourceStackSaveData> savedResources
    )
    {
        if (savedResources == null)
            return;

        foreach (ResourceStackSaveData savedResource in savedResources)
        {
            ResourceDefinition resource = ResolveResource(savedResource);

            if (resource != null)
            {
                productionSystem.RestoreGlobalResource(
                    resource,
                    savedResource.amount
                );
            }
        }
    }

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

    private void RestoreFinance(GameSaveData saveData)
    {
        if (saveData.incomeRecords != null)
        {
            foreach (IncomeRecordSaveData record in saveData.incomeRecords)
            {
                if (record == null)
                    continue;

                productionSystem.RestoreIncomeRecord(
                    new IncomeRecord(
                        record.buildingName,
                        record.soldResources,
                        record.income,
                        record.moneyAfterSale,
                        record.gameMinutes
                    )
                );
            }
        }

        if (saveData.expenseRecords != null)
        {
            foreach (ExpenseRecordSaveData record in saveData.expenseRecords)
            {
                if (record == null)
                    continue;

                productionSystem.RestoreExpenseRecord(
                    new ExpenseRecord(
                        record.sourceName,
                        record.expenseType,
                        record.paidResources,
                        record.expense,
                        record.moneyAfterExpense,
                        record.gameMinutes
                    )
                );
            }
        }
        if (saveData.monthlyFinanceRecords != null)
        {
            foreach (
                MonthlyFinanceRecordSaveData record
                in saveData.monthlyFinanceRecords
            )
            {
                if (record == null)
                    continue;

                productionSystem.RestoreMonthlyFinanceRecord(
                    record.month,
                    record.income,
                    record.expense
                );
            }
        }
    }

    private void RestoreRoutes(
        GameSaveData saveData,
        HashSet<string> restoredBuildingIds
    )
    {
        List<TransportRouteDefinition> restoredRoutes =
            new List<TransportRouteDefinition>();

        if (saveData.transportRoutes != null)
        {
            foreach (
                TransportRouteSaveData savedRoute
                in saveData.transportRoutes
            )
            {
                if (
                    savedRoute == null ||
                    !restoredBuildingIds.Contains(
                        savedRoute.sourceBuildingId
                    ) ||
                    !restoredBuildingIds.Contains(
                        savedRoute.targetBuildingId
                    )
                )
                {
                    continue;
                }

                ResourceDefinition resource = null;

                if (!string.IsNullOrWhiteSpace(savedRoute.resourceId))
                {
                    resource = definitionCatalog.FindResource(
                        savedRoute.resourceId
                    );

                    if (resource == null)
                    {
                        Debug.LogWarning(
                            $"Route resource not found: {savedRoute.resourceId}"
                        );
                    }
                }

                restoredRoutes.Add(new TransportRouteDefinition
                {
                    routeId = savedRoute.routeId,
                    displayName = savedRoute.displayName,
                    sourceBuildingId = savedRoute.sourceBuildingId,
                    targetBuildingId = savedRoute.targetBuildingId,
                    resource = resource,
                    amountPerTransfer =
                        Mathf.Max(1, savedRoute.amountPerTransfer),
                    enabled = savedRoute.enabled
                });
            }
        }

        transportSystem.RestoreState(
            restoredRoutes,
            saveData.nextRouteIndex,
            saveData.lastTransportGameMinutes
        );
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

    private ResourceDefinition ResolveResource(
        ResourceStackSaveData savedResource
    )
    {
        if (savedResource == null || savedResource.amount <= 0)
            return null;

        ResourceDefinition resource =
            definitionCatalog.FindResource(savedResource.resourceId);

        if (resource == null)
        {
            Debug.LogWarning(
                $"Resource definition not found: " +
                $"{savedResource.resourceId}"
            );
        }

        return resource;
    }
}
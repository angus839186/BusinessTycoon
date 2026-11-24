using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public sealed partial class GameSaveSystem
{

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
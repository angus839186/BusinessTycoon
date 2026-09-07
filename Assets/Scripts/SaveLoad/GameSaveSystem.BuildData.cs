public sealed partial class GameSaveSystem
{
    private GameSaveData CreateSaveData()
    {
        GameSaveData data = new GameSaveData();

        if (gameClock != null)
        {
            data.totalGameMinutes = gameClock.TotalGameMinutes;
            data.speedMultiplier = gameClock.SpeedMultiplier;
        }

        if (buildingPlacement != null)
            data.nextBuildingInstanceIndex =
                buildingPlacement.NextBuildingInstanceIndex;

        if (transportSystem != null)
        {
            data.nextRouteIndex = transportSystem.NextRouteIndex;
            data.lastTransportGameMinutes =
                transportSystem.LastTransferGameMinutes;

            foreach (TransportRouteDefinition route in transportSystem.Routes)
            {
                if (route == null)
                    continue;

                data.transportRoutes.Add(new TransportRouteSaveData
                {
                    routeId = route.routeId,
                    displayName = route.displayName,
                    sourceBuildingId = route.sourceBuildingId,
                    targetBuildingId = route.targetBuildingId,
                    resourceId = route.resource != null
                        ? route.resource.resourceId
                        : "",
                    amountPerTransfer = route.amountPerTransfer,
                    enabled = route.enabled
                });
            }
        }

        if (productionSystem == null)
            return data;

        AddInventory(
            productionSystem.GlobalInventory,
            data.globalResources
        );

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null || building.Definition == null)
                continue;

            BuildingSaveData buildingData = new BuildingSaveData
            {
                instanceId = building.InstanceId,
                buildingId = building.Definition.buildingId,
                worldPosition = building.WorldPosition,
                longitude = building.Longitude,
                latitude = building.Latitude,
                lastMaintenanceGameMinutes =
                    building.LastMaintenanceGameMinutes,
                lastProcessGameMinutes =
                    building.LastProcessGameMinutes
            };

            AddInventory(building.Inventory, buildingData.inventory);

            foreach (ResourceAmount cost in building.ConstructionCostSnapshot)
            {
                if (cost == null || cost.resource == null || cost.amount <= 0)
                    continue;

                buildingData.constructionCostSnapshot.Add(
                    CreateResourceData(cost.resource, cost.amount)
                );
            }

            data.buildings.Add(buildingData);
        }

        foreach (IncomeRecord record in productionSystem.IncomeRecords)
        {
            data.incomeRecords.Add(new IncomeRecordSaveData
            {
                buildingName = record.buildingName,
                soldResources = record.soldResources,
                income = record.income,
                moneyAfterSale = record.moneyAfterSale,
                gameMinutes = record.gameMinutes
            });
        }

        foreach (ExpenseRecord record in productionSystem.ExpenseRecords)
        {
            data.expenseRecords.Add(new ExpenseRecordSaveData
            {
                sourceName = record.sourceName,
                expenseType = record.expenseType,
                paidResources = record.paidResources,
                expense = record.expense,
                moneyAfterExpense = record.moneyAfterExpense,
                gameMinutes = record.gameMinutes
            });
        }

        return data;
    }

    private static void AddInventory(
        BuildingInventory inventory,
        System.Collections.Generic.List<ResourceStackSaveData> output
    )
    {
        if (inventory == null)
            return;

        foreach (ResourceStack stack in inventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
                continue;

            output.Add(CreateResourceData(stack.resource, stack.amount));
        }
    }

    private static ResourceStackSaveData CreateResourceData(
        ResourceDefinition resource,
        int amount
    )
    {
        return new ResourceStackSaveData
        {
            resourceId = resource.resourceId,
            amount = amount
        };
    }
}
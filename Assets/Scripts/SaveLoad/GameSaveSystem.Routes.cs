using System.Collections.Generic;
using UnityEngine;

public sealed partial class GameSaveSystem
{
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
}
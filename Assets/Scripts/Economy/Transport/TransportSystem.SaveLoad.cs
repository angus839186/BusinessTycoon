using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TransportSystem
{
    public void RestoreState(
        IReadOnlyList<TransportRouteDefinition> restoredRoutes,
        int restoredNextRouteIndex,
        double restoredLastTransferGameMinutes
    )
    {
        if (restoredRoutes == null || restoredRoutes.Count == 0)
        {
            routes = Array.Empty<TransportRouteDefinition>();
        }
        else
        {
            List<TransportRouteDefinition> validRoutes = new List<TransportRouteDefinition>();
            HashSet<string> usedSources = new HashSet<string>();
            HashSet<string> usedTargets = new HashSet<string>();

            foreach (TransportRouteDefinition route in restoredRoutes)
            {
                if (route == null || string.IsNullOrWhiteSpace(
                    route.sourceBuildingId
                    ) || string.IsNullOrWhiteSpace(route.targetBuildingId)
                )
                {
                    Debug.LogWarning(
                        "Skipped invalid restored route."
                    );

                    continue;
                }

                if (route.sourceBuildingId == route.targetBuildingId)
                {
                    Debug.LogWarning(
                        $"Skipped route with identical " +
                        $"source and target: {route.routeId}"
                    );

                    continue;
                }

                if (usedSources.Contains(route.sourceBuildingId))
                {
                    Debug.LogWarning(
                        $"Skipped duplicate outgoing route: " +
                        $"{route.routeId}"
                    );

                    continue;
                }

                if (usedTargets.Contains(route.targetBuildingId))
                {
                    Debug.LogWarning(
                        $"Skipped duplicate incoming route: " +
                        $"{route.routeId}"
                    );

                    continue;
                }

                usedSources.Add(route.sourceBuildingId);
                usedTargets.Add(route.targetBuildingId);
                validRoutes.Add(route);
            }

            routes = validRoutes.ToArray();
        }

        useDefinedRoutes = routes.Length > 0;
        nextRouteIndex = Mathf.Max(1, restoredNextRouteIndex);
        lastTransferGameMinutes = Math.Max(0, restoredLastTransferGameMinutes);

        transportRecords.Clear();

        MarkRoutesChanged();
        MarkTransportChanged();
    }
}
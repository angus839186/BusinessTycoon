using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TransportSystem
{
    public void AddDefinedRoute(
    BuildingInstance source,
    BuildingInstance target
)
    {
        if (
            source == null ||
            target == null ||
            source == target ||
            source.Definition == null ||
            target.Definition == null
        )
        {
            return;
        }

        TransportRouteDefinition route = new TransportRouteDefinition
        {
            routeId = $"route_{nextRouteIndex}",
            displayName =
                $"{source.Definition.displayName} -> " +
                $"{target.Definition.displayName}",
            sourceBuildingId = source.InstanceId,
            targetBuildingId = target.InstanceId,
            resource = null,
            amountPerTransfer = 1,
            enabled = true
        };

        List<TransportRouteDefinition> routeList =
            routes != null
                ? new List<TransportRouteDefinition>(routes)
                : new List<TransportRouteDefinition>();

        routeList.Add(route);
        routes = routeList.ToArray();

        nextRouteIndex++;
        useDefinedRoutes = true;
        MarkRoutesChanged();

        Debug.Log(
            $"Route Created: {route.displayName}, Resource=Not selected"
        );
    }
    public void ClearRoutes()
    {
        routes = Array.Empty<TransportRouteDefinition>();
        useDefinedRoutes = false;
        MarkRoutesChanged();

        Debug.Log("Transport routes cleared.");
    }

    private void RefreshRouteVisuals()
    {
        if (routeVisualizer == null)
            return;

        routeVisualizer.ClearRoutes();

        if (routes == null || pathProvider == null)
            return;

        foreach (TransportRouteDefinition route in routes)
        {
            if (route == null)
                continue;

            BuildingInstance source =
                FindBuildingById(route.sourceBuildingId);

            BuildingInstance target =
                FindBuildingById(route.targetBuildingId);

            if (source == null || target == null)
                continue;

            IReadOnlyList<Vector3> path =
                pathProvider.GetPath(source, target);

            routeVisualizer.ShowRoute(
                route.routeId,
                path
            );
        }
    }
}
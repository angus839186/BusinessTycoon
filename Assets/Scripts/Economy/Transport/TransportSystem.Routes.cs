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
    // SetRouteResource
    // SetRouteAmount
    // SetRouteEnabled
    // RemoveRoutesForBuilding
    // RemoveRoute
    // FindRouteById
    // GetAvailableRouteResources
    // FindBuildingById
}
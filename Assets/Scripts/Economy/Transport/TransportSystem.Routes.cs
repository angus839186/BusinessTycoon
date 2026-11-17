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
        if (!TryAddDefinedRoute(
                source,
                target,
                out string errorMessage
            ))
        {
            Debug.LogWarning(
                errorMessage,
                this
            );
        }
    }
    public bool TryAddDefinedRoute(
        BuildingInstance source,
        BuildingInstance target,
        out string errorMessage
    )
    {
        errorMessage = string.Empty;

        if (
            source == null ||
            target == null ||
            source.Definition == null ||
            target.Definition == null
        )
        {
            errorMessage =
                "無法建立路線：建築資料無效。";

            return false;
        }

        if (source == target)
        {
            errorMessage =
                "起點與終點不能是同一棟建築。";

            return false;
        }

        if (HasOutgoingRoute(source.InstanceId))
        {
            errorMessage =
                $"{source.Definition.displayName} 已有輸出路線。";

            return false;
        }

        if (HasIncomingRoute(target.InstanceId))
        {
            errorMessage =
                $"{target.Definition.displayName} 已有輸入路線。";

            return false;
        }

        TransportRouteDefinition route =
            new TransportRouteDefinition
            {
                routeId =
                    $"route_{nextRouteIndex}",

                displayName =
                    $"{source.Definition.displayName} -> " +
                    $"{target.Definition.displayName}",

                sourceBuildingId =
                    source.InstanceId,

                targetBuildingId =
                    target.InstanceId,

                resource = null,
                amountPerTransfer = 1,
                enabled = true
            };

        List<TransportRouteDefinition> routeList =
            routes != null
                ? new List<TransportRouteDefinition>(
                    routes
                )
                : new List<TransportRouteDefinition>();

        routeList.Add(route);
        routes = routeList.ToArray();

        nextRouteIndex++;
        useDefinedRoutes = true;

        MarkRoutesChanged();
        RouteCreated?.Invoke(route);

        Debug.Log(
            $"Route Created: {route.displayName}, " +
            "Resource=Not selected"
        );

        return true;
    }

    public bool HasOutgoingRoute(
    string buildingId
)
    {
        if (
            string.IsNullOrWhiteSpace(buildingId) ||
            routes == null
        )
        {
            return false;
        }

        foreach (
            TransportRouteDefinition route in routes
        )
        {
            if (
                route != null &&
                route.sourceBuildingId == buildingId
            )
            {
                return true;
            }
        }

        return false;
    }

    public bool HasIncomingRoute(
        string buildingId
    )
    {
        if (
            string.IsNullOrWhiteSpace(buildingId) ||
            routes == null
        )
        {
            return false;
        }

        foreach (
            TransportRouteDefinition route in routes
        )
        {
            if (
                route != null &&
                route.targetBuildingId == buildingId
            )
            {
                return true;
            }
        }

        return false;
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
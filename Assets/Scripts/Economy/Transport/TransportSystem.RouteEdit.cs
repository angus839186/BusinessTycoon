using System.Collections.Generic;
using UnityEngine;

public sealed partial class TransportSystem
{
    public bool SetRouteResource(string routeId, ResourceDefinition resource)
    {
        if (resource == null)
            return false;

        TransportRouteDefinition route = FindRouteById(routeId);

        if (route == null)
            return false;

        List<ResourceDefinition> availableResources = GetAvailableRouteResources(route);

        if (!availableResources.Contains(resource))
            return false;

        route.resource = resource;
        MarkRoutesChanged();

        Debug.Log($"Transport route resource changed: {routeId}, Resource={resource.displayName}");
        return true;
    }

    public bool SetRouteAmount(string routeId, int amountPerTransfer)
    {
        TransportRouteDefinition route = FindRouteById(routeId);

        if (route == null)
            return false;

        route.amountPerTransfer = Mathf.Max(1, amountPerTransfer);
        MarkRoutesChanged();

        Debug.Log($"Transport route amount changed: {routeId}, Amount={route.amountPerTransfer}");
        return true;
    }
    public bool SetRouteEnabled(string routeId, bool enabled)
    {
        TransportRouteDefinition route = FindRouteById(routeId);

        if (route == null)
            return false;

        route.enabled = enabled;
        MarkRoutesChanged();

        Debug.Log($"Transport route {(enabled ? "enabled" : "disabled")}: {routeId}");
        return true;
    }

    public void RemoveRoutesForBuilding(string buildingId)
    {
        if (string.IsNullOrWhiteSpace(buildingId) || routes == null || routes.Length == 0)
            return;

        List<TransportRouteDefinition> routeList =
            new List<TransportRouteDefinition>(routes);

        bool changed = false;

        for (int i = routeList.Count - 1; i >= 0; i--)
        {
            TransportRouteDefinition route = routeList[i];

            if (route == null)
                continue;

            if (
                route.sourceBuildingId == buildingId ||
                route.targetBuildingId == buildingId
            )
            {
                routeList.RemoveAt(i);
                changed = true;
            }
        }

        if (!changed)
            return;

        routes = routeList.ToArray();

        if (routes.Length == 0)
            useDefinedRoutes = false;

        MarkRoutesChanged();
    }

    public bool RemoveRoute(string routeId)
    {
        if (string.IsNullOrWhiteSpace(routeId) || routes == null || routes.Length == 0)
            return false;

        List<TransportRouteDefinition> routeList =
            new List<TransportRouteDefinition>(routes);

        for (int i = routeList.Count - 1; i >= 0; i--)
        {
            TransportRouteDefinition route = routeList[i];

            if (route == null || route.routeId != routeId)
                continue;

            routeList.RemoveAt(i);
            routes = routeList.ToArray();

            if (routes.Length == 0)
                useDefinedRoutes = false;
            MarkRoutesChanged();

            Debug.Log($"Transport route removed: {routeId}");
            return true;
        }

        return false;
    }

    public TransportRouteDefinition FindRouteById(string routeId)
    {
        if (string.IsNullOrWhiteSpace(routeId) || routes == null)
            return null;

        foreach (TransportRouteDefinition route in routes)
        {
            if (route == null)
                continue;

            if (route.routeId == routeId)
                return route;
        }

        return null;
    }
    public List<ResourceDefinition> GetAvailableRouteResources(TransportRouteDefinition route)
    {
        List<ResourceDefinition> availableResources = new List<ResourceDefinition>();

        if (route == null)
            return availableResources;

        BuildingInstance source = FindBuildingById(route.sourceBuildingId);
        BuildingInstance target = FindBuildingById(route.targetBuildingId);

        if (source == null || target == null || target.Definition == null)
            return availableResources;

        foreach (ResourceStack stack in source.Inventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
                continue;

            if (!NeedsResource(target.Definition, stack.resource))
                continue;

            if (availableResources.Contains(stack.resource))
                continue;

            availableResources.Add(stack.resource);
        }

        return availableResources;
    }
}
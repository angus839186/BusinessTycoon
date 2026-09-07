using System.Collections.Generic;
using UnityEngine;

public sealed partial class TransportSystem
{
    private void TransferOnce()
    {
        foreach (BuildingInstance source in productionSystem.Buildings)
        {
            if (source == null || source.Definition == null)
                continue;

            foreach (ResourceStack stack in source.Inventory.Resources)
            {
                if (stack == null || stack.resource == null || stack.amount <= 0)
                    continue;

                if (TrySendResource(source, stack.resource))
                    return;
            }
        }
    }

    private void TransferDefinedRouteOnce()
    {
        if (routes == null || routes.Length == 0)
            return;

        foreach (TransportRouteDefinition route in routes)
        {
            if (route == null || !route.enabled)
                continue;

            TryTransferRoute(route);
        }
    }

    private bool TryTransferRoute(TransportRouteDefinition route)
    {
        if (
            route.resource == null ||
            route.amountPerTransfer <= 0 ||
            string.IsNullOrWhiteSpace(route.sourceBuildingId) ||
            string.IsNullOrWhiteSpace(route.targetBuildingId)
        )
            return false;

        BuildingInstance source = FindBuildingById(route.sourceBuildingId);
        BuildingInstance target = FindBuildingById(route.targetBuildingId);

        if (source == null || target == null)
            return false;

        if (!NeedsResource(target.Definition, route.resource))
            return false;

        if (!source.Inventory.TryConsume(route.resource, route.amountPerTransfer))
            return false;

        target.Inventory.Add(route.resource, route.amountPerTransfer);

        IReadOnlyList<Vector3> path = pathProvider != null
            ? pathProvider.GetPath(source, target)
            : null;

        AddTransportRecord(
            source,
            target,
            route.resource,
            route.amountPerTransfer,
            path
        );
        MarkTransportChanged();

        if (path != null && routeVisualizer != null)
            routeVisualizer.ShowTransferPath(path);

        if (logTransfer)
        {
            string routeName = string.IsNullOrWhiteSpace(route.displayName)
                ? route.routeId
                : route.displayName;

            Debug.Log(
                $"Route Transferred: Route={routeName}, " +
                $"Resource={route.resource.displayName} x{route.amountPerTransfer}, " +
                $"From={source.Definition.displayName}, " +
                $"To={target.Definition.displayName}"
            );
        }

        return true;
    }

    private bool TrySendResource(
        BuildingInstance source,
        ResourceDefinition resource
    )
    {
        foreach (BuildingInstance target in productionSystem.Buildings)
        {
            if (target == null || target == source || target.Definition == null)
                continue;

            if (!NeedsResource(target.Definition, resource))
                continue;

            if (!source.Inventory.TryConsume(resource, transferAmount))
                return false;

            target.Inventory.Add(resource, transferAmount);

            IReadOnlyList<Vector3> path = pathProvider != null
                ? pathProvider.GetPath(source, target)
                : null;

            AddTransportRecord(source, target, resource, transferAmount, path);
            MarkTransportChanged();

            if (path != null && routeVisualizer != null)
                routeVisualizer.ShowTransferPath(path);
            if (logTransfer)
            {
                Debug.Log(
                    $"Transferred: {resource.displayName} x{transferAmount}, " +
                    $"From={source.Definition.displayName}, " +
                    $"To={target.Definition.displayName}"
                );
            }

            return true;
        }

        return false;
    }
    private void AddTransportRecord(
    BuildingInstance source,
    BuildingInstance target,
    ResourceDefinition resource,
    int amount,
    IReadOnlyList<Vector3> path
)
    {
        if (
            source == null ||
            target == null ||
            source.Definition == null ||
            target.Definition == null ||
            resource == null ||
            amount <= 0
        )
            return;

        TransportRecord record = new TransportRecord(
    resource.displayName,
    amount,
    source.InstanceId,
    target.InstanceId,
    source.Definition.displayName,
    target.Definition.displayName,
    source.transform.position,
    target.transform.position,
    path != null ? path.Count : 0,
    gameClock != null ? gameClock.TotalGameMinutes : 0
);

        transportRecords.Insert(0, record);

        while (transportRecords.Count > maxTransportRecords)
            transportRecords.RemoveAt(transportRecords.Count - 1);
    }
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
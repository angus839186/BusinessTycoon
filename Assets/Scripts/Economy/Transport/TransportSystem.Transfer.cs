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

        if (source == null || target == null) return false;
        if (!source.IsOperational || !target.IsOperational) return false;

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
        if (source == null || !source.IsOperational) return false;
        foreach (BuildingInstance target in productionSystem.Buildings)
        {
            if (
                target == null ||
                target == source ||
                target.Definition == null ||
                !target.IsOperational
            )
            {
                continue;
            }

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

}
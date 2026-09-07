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
            routes = new TransportRouteDefinition[restoredRoutes.Count];

            for (int i = 0; i < restoredRoutes.Count; i++)
                routes[i] = restoredRoutes[i];
        }

        useDefinedRoutes = routes.Length > 0;
        nextRouteIndex = Mathf.Max(1, restoredNextRouteIndex);
        lastTransferGameMinutes =
            Math.Max(0, restoredLastTransferGameMinutes);

        transportRecords.Clear();

        MarkRoutesChanged();
        MarkTransportChanged();
    }
}
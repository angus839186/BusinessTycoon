using System.Collections.Generic;

public sealed partial class TransportReportPanel
{
    public void ClearRoutes()
    {
        if (transport == null) return;
        transport.ClearRoutes();
    }
    public void EnableRoute(string routeId)
    {
        if (transport == null) return;
        transport.SetRouteEnabled(routeId, true);
    }

    public void DisableRoute(string routeId)
    {
        if (transport == null) return;
        transport.SetRouteEnabled(routeId, false);
    }

    public void RemoveRoute(string routeId)
    {
        if (transport == null) return;
        transport.RemoveRoute(routeId);
    }

    public void SetRouteAmount(string routeId, int amountPerTransfer)
    {
        if (transport == null) return;
        transport.SetRouteAmount(routeId, amountPerTransfer);
    }
    public void SetRouteResource(string routeId, ResourceDefinition resource)
    {
        if (transport == null) return;
        transport.SetRouteResource(routeId, resource);
    }

    public List<ResourceDefinition> GetAvailableRouteResources(TransportRouteDefinition route)
    {
        if (transport == null) return new List<ResourceDefinition>();
        return transport.GetAvailableRouteResources(route);
    }


    private void HandleCreateRouteClicked()
    {
        if (mapMode == null)
        {
            return;
        }

        if (panelController != null)
        {
            panelController.CloseCurrentPanel();
        }

        mapMode.StartRouteMode();
    }
}
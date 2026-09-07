using UnityEngine;
using System.Collections.Generic;

public sealed class TransportReportPanel : MonoBehaviour
{
    [SerializeField] private TransportSystem transport;

    [SerializeField] private Transform routeListRoot;
    [SerializeField] private TransportRouteListItemUI routeItemPrefab;
    [SerializeField] private ProductionSystem productionSystem;

    private int lastRouteVersion = -1;

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();
    }
    private void OnEnable()
    {
        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();

        if (transport != null)
            transport.RoutesChanged += Refresh;
        if (productionSystem != null)
            productionSystem.DataChanged += RefreshRouteListNow;

        if (transport != null)
            transport.TransportChanged += RefreshRouteListNow;

        lastRouteVersion = -1;
        Refresh();
    }

    private void OnDisable()
    {
        if (transport != null)
            transport.RoutesChanged -= Refresh;
        if (productionSystem != null)
            productionSystem.DataChanged -= RefreshRouteListNow;

        if (transport != null)
            transport.TransportChanged -= RefreshRouteListNow;
    }

    private void Refresh()
    {
        RefreshRouteListIfNeeded();
    }
    public void ClearRoutes()
    {
        if (transport == null)
            return;

        transport.ClearRoutes();
        Refresh();
    }
    public void EnableRoute(string routeId)
    {
        if (transport == null)
            return;

        transport.SetRouteEnabled(routeId, true);
        Refresh();
    }

    public void DisableRoute(string routeId)
    {
        if (transport == null)
            return;

        transport.SetRouteEnabled(routeId, false);
        Refresh();
    }

    public void RemoveRoute(string routeId)
    {
        if (transport == null)
            return;

        transport.RemoveRoute(routeId);
        Refresh();
    }

    public void SetRouteAmount(string routeId, int amountPerTransfer)
    {
        if (transport == null)
            return;

        transport.SetRouteAmount(routeId, amountPerTransfer);
        Refresh();
    }
    public void SetRouteResource(string routeId, ResourceDefinition resource)
    {
        if (transport == null)
            return;

        transport.SetRouteResource(routeId, resource);
        Refresh();
    }

    public List<ResourceDefinition> GetAvailableRouteResources(TransportRouteDefinition route)
    {
        if (transport == null)
            return new List<ResourceDefinition>();

        return transport.GetAvailableRouteResources(route);
    }
    private void RefreshRouteList()
    {
        if (routeListRoot == null || routeItemPrefab == null || transport == null)
            return;

        for (int i = routeListRoot.childCount - 1; i >= 0; i--)
            Destroy(routeListRoot.GetChild(i).gameObject);

        foreach (TransportRouteDefinition route in transport.Routes)
        {
            if (route == null)
                continue;

            TransportRouteListItemUI item =
                Instantiate(routeItemPrefab, routeListRoot);

            item.gameObject.SetActive(true);
            item.Initialize(this, route);
        }
    }
    private void RefreshRouteListIfNeeded()
    {
        if (transport == null)
            return;

        if (lastRouteVersion == transport.RouteVersion)
            return;

        lastRouteVersion = transport.RouteVersion;
        RefreshRouteList();
    }
    private void RefreshRouteListNow()
    {
        lastRouteVersion = -1;
        Refresh();
    }
}
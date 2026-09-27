using UnityEngine;
using System.Collections.Generic;

public sealed class TransportReportPanel : MonoBehaviour
{
    [SerializeField] private TransportSystem transport;

    [SerializeField] private Transform routeListRoot;
    [SerializeField] private TransportRouteListItemUI routeItemPrefab;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private GameObject emptyText;

    private ReusableUIList<TransportRouteListItemUI> routeItemList;

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
        if (routeItemPrefab != null && routeListRoot != null)
        {
            routeItemList =
                new ReusableUIList<TransportRouteListItemUI>(
                    routeItemPrefab,
                    routeListRoot
                );
        }

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
    private void RefreshRouteList()
    {
        if (transport == null || routeItemList == null) return;

        int itemIndex = 0;

        foreach (TransportRouteDefinition route in transport.Routes)
        {
            if (route == null) continue;

            TransportRouteListItemUI item =
                routeItemList.GetOrCreate(itemIndex);

            if (item == null) continue;

            item.Initialize(this, route);
            itemIndex++;
        }

        routeItemList.HideFrom(itemIndex);

        if (emptyText != null) emptyText.SetActive(itemIndex == 0);
    }
    private void RefreshRouteListIfNeeded()
    {
        if (transport == null) return;

        if (lastRouteVersion == transport.RouteVersion) return;

        lastRouteVersion = transport.RouteVersion;
        RefreshRouteList();
    }
    private void RefreshRouteListNow()
    {
        lastRouteVersion = -1;
        Refresh();
    }
}
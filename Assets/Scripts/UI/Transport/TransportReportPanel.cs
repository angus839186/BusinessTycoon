using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

public sealed class TransportReportPanel : MonoBehaviour
{
    [SerializeField] private TransportSystem transport;

    [SerializeField] private PanelController panelController;
    [SerializeField] private MapModeController mapMode;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private Transform routeListRoot;
    [SerializeField] private TransportRouteItem routeItemPrefab;
    [SerializeField] private GameObject emptyText;

    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private TMP_Dropdown statusDropdown;
    [SerializeField] private Button createRouteButton;


    private ReusableUIList<TransportRouteItem> routeItemList;

    private int lastRouteVersion = -1;

    private void Awake()
    {
        if (panelController == null)
        {
            panelController = FindFirstObjectByType<PanelController>();
        }

        if (mapMode == null)
        {
            mapMode = FindFirstObjectByType<MapModeController>();
        }

        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();

        if (routeItemPrefab != null && routeListRoot != null)
        {
            routeItemList =
                new ReusableUIList<TransportRouteItem>(
                    routeItemPrefab,
                    routeListRoot
                );
        }
        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(HandleSearchChanged);
        }

        if (statusDropdown != null)
        {
            statusDropdown.onValueChanged.AddListener(HandleStatusChanged);
        }
        if (createRouteButton != null)
        {
            createRouteButton.onClick.AddListener(
                HandleCreateRouteClicked
            );
        }

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

    private void OnDestroy()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(
                HandleSearchChanged
            );
        }

        if (statusDropdown != null)
        {
            statusDropdown.onValueChanged.RemoveListener(
                HandleStatusChanged
            );
        }
        if (createRouteButton != null)
        {
            createRouteButton.onClick.RemoveListener(
                HandleCreateRouteClicked
            );
        }
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
            if (!MatchesFilter(route)) continue;

            TransportRouteItem item =
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

    private bool MatchesFilter(TransportRouteDefinition route)
    {
        if (route == null) return false;

        int statusIndex =
            statusDropdown != null
                ? statusDropdown.value
                : 0;

        bool matchesStatus = statusIndex switch
        {
            0 => true,
            1 => route.enabled,
            2 => !route.enabled,
            _ => true
        };

        if (!matchesStatus) return false;

        string query = searchInput != null ? searchInput.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        return
            ContainsText(route.routeId, query) ||
            ContainsText(route.displayName, query) ||
            ContainsText(route.sourceBuildingId, query) ||
            ContainsText(route.targetBuildingId, query) ||
            (
                route.resource != null &&
                (
                    ContainsText(
                        route.resource.displayName,
                        query
                    ) ||
                    ContainsText(
                        route.resource.resourceId,
                        query
                    )
                )
            );
    }

    private static bool ContainsText(string value, string query)
    {
        return
            !string.IsNullOrEmpty(value) &&
            value.IndexOf(
                query,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;
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

    private void HandleSearchChanged(string value)
    {
        RefreshRouteList();
    }

    private void HandleStatusChanged(int value)
    {
        RefreshRouteList();
    }
}
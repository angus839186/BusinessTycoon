using System;

public sealed partial class TransportReportPanel
{
    private ReusableUIList<TransportRouteItem> routeItemList;

    private int lastRouteVersion = -1;

    private void Refresh()
    {
        RefreshRouteListIfNeeded();
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

    private void HandleSearchChanged(string value)
    {
        RefreshRouteList();
    }

    private void HandleStatusChanged(int value)
    {
        RefreshRouteList();
    }

}
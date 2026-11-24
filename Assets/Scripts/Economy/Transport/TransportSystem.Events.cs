using System;
public sealed partial class TransportSystem
{
    public event Action RoutesChanged;
    public event Action TransportChanged;
    public event Action<TransportRouteDefinition> RouteCreated;

    private void MarkRoutesChanged()
    {
        RefreshRouteVisuals();

        RouteVersion++;
        RoutesChanged?.Invoke();
    }
    private void MarkTransportChanged()
    {
        TransportChanged?.Invoke();
    }
}
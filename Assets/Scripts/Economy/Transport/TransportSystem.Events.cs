using UnityEngine;

public sealed partial class TransportSystem
{
    private void MarkRoutesChanged()
    {
        RouteVersion++;
        RoutesChanged?.Invoke();
    }
    private void MarkTransportChanged()
    {
        TransportChanged?.Invoke();
    }

    public void ClearTransportRecords()
    {
        transportRecords.Clear();
        MarkTransportChanged();

        Debug.Log("Transport records cleared.");
    }
}
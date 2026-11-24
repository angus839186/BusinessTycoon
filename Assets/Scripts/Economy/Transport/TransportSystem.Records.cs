using System.Collections.Generic;
using UnityEngine;

public sealed partial class TransportSystem
{
    private readonly List<TransportRecord> transportRecords = new List<TransportRecord>();

    public IReadOnlyList<TransportRecord> TransportRecords => transportRecords;

    private void AddTransportRecord(
        BuildingInstance source,
        BuildingInstance target,
        ResourceDefinition resource,
        int amount,
        IReadOnlyList<Vector3> path)
    {
        if (source == null ||
            target == null ||
            source.Definition == null ||
            target.Definition == null ||
            resource == null ||
            amount <= 0)
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
            gameClock != null ? gameClock.TotalGameMinutes : 0);

        transportRecords.Insert(0, record);

        while (transportRecords.Count > maxTransportRecords)
            transportRecords.RemoveAt(transportRecords.Count - 1);
    }

    public void ClearTransportRecords()
    {
        transportRecords.Clear();
        MarkTransportChanged();

        Debug.Log("Transport records cleared.");
    }
}
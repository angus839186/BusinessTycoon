using System;

[Serializable]
public sealed class TransportRouteSaveData
{
    public string routeId;
    public string displayName;

    public string sourceBuildingId;
    public string targetBuildingId;
    public string resourceId;

    public int amountPerTransfer;
    public bool enabled;
}
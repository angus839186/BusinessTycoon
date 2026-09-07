using System;
using UnityEngine;

[Serializable]
public sealed class TransportRouteDefinition
{
    public string routeId;
    public string displayName;

    public string sourceBuildingId;
    public string targetBuildingId;

    public ResourceDefinition resource;
    public int amountPerTransfer = 1;

    public bool enabled = true;
}
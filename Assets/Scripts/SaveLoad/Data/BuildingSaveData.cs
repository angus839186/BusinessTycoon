using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BuildingSaveData
{
    public string instanceId;
    public string buildingId;

    public Vector3 worldPosition;
    public double longitude;
    public double latitude;

    public double lastMaintenanceGameMinutes;
    public double lastProcessGameMinutes;

    public List<ResourceStackSaveData> inventory =
        new List<ResourceStackSaveData>();

    public List<ResourceStackSaveData> constructionCostSnapshot =
        new List<ResourceStackSaveData>();
}
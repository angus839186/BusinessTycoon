using System;
using UnityEngine;

[Serializable]
public sealed class LocationValueArea
{
    public string areaId;
    public string displayName;

    public double longitude;
    public double latitude;
    public double radiusDegrees = 0.5;

    [Range(0f, 1f)]
    public float population01 = 0.5f;
}
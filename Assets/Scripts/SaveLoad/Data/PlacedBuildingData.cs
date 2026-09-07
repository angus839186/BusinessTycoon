using UnityEngine;

[System.Serializable]
public sealed class PlacedBuildingData
{
    public string buildingId;
    public string buildingDisplayName;

    public double longitude;
    public double latitude;

    public Vector3 worldPosition;
}
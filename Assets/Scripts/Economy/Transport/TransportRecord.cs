using UnityEngine;

public sealed class TransportRecord
{
    public string resourceName;
    public int amount;
    public string sourceBuildingName;
    public string targetBuildingName;
    public string sourceBuildingId;
    public string targetBuildingId;
    public double gameMinutes;

    public Vector3 sourcePosition;
    public Vector3 targetPosition;
    public int pathPointCount;

    public TransportRecord(
    string resourceName,
    int amount,
    string sourceBuildingId,
    string targetBuildingId,
    string sourceBuildingName,
    string targetBuildingName,
    Vector3 sourcePosition,
    Vector3 targetPosition,
    int pathPointCount,
    double gameMinutes
)
    {
        this.resourceName = resourceName;
        this.amount = amount;
        this.sourceBuildingName = sourceBuildingName;
        this.targetBuildingName = targetBuildingName;
        this.sourcePosition = sourcePosition;
        this.targetPosition = targetPosition;
        this.pathPointCount = pathPointCount;
        this.sourceBuildingId = sourceBuildingId;
        this.targetBuildingId = targetBuildingId;
        this.gameMinutes = gameMinutes;
    }
}
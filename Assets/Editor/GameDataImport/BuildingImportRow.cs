using UnityEngine;

public class BuildingImportRow
{
    public int LineNumber { get; }
    public string BuildingId { get; }
    public string DisplayName { get; }
    public BuildingCategory Category { get; }
    public string ProducedResourceId { get; }
    public GameTimeInterval ProcessInterval { get; }
    public float WorkSpeedMultiplier { get; }
    public bool AcceptsAnyResource { get; }
    public Color MarkerColor { get; }
    public string IconPath { get; }
    public bool Enabled { get; }

    public BuildingImportRow(
        int lineNumber,
        string buildingId,
        string displayName,
        BuildingCategory category,
        string producedResourceId,
        GameTimeInterval processInterval,
        float workSpeedMultiplier,
        bool acceptsAnyResource,
        Color markerColor,
        string iconPath,
        bool enabled
    )
    {
        LineNumber = lineNumber;
        BuildingId = buildingId;
        DisplayName = displayName;
        Category = category;
        ProducedResourceId = producedResourceId;
        ProcessInterval = processInterval;
        WorkSpeedMultiplier = workSpeedMultiplier;
        AcceptsAnyResource = acceptsAnyResource;
        MarkerColor = markerColor;
        IconPath = iconPath;
        Enabled = enabled;
    }
}
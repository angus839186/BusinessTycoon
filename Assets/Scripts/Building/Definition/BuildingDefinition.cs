using System.ComponentModel;
using UnityEngine;

public enum BuildingCategory
{
    [InspectorName("資源建築")]
    Resource,
    [InspectorName("加工建築")]
    Processing,
    [InspectorName("商業建築")]
    Commercial,

    [InspectorName("倉管建築")]
    Storage
}
[GameDataTable(
    "Buildings.tsv",
    "Assets/Resources/Buildings",
    "buildingId"
)]
[GameDataColumnAlias(
    "producedResourceId",
    "producedResource"
)]
[GameDataColumnAlias(
    "processMonths",
    "processInterval.months"
)]
[GameDataColumnAlias(
    "processDays",
    "processInterval.days"
)]
[GameDataColumnAlias(
    "processHours",
    "processInterval.hours"
)]
[GameDataColumnAlias(
    "processMinutes",
    "processInterval.minutes"
)]
[GameDataColumnAlias("iconPath", "icon")]
[GameDataColumnAlias("enabled", "isEnabled")]
[CreateAssetMenu(
    fileName = "BuildingDefinition",
    menuName = "WorldMap/Building Definition"
)]
public sealed class BuildingDefinition : ScriptableObject
{
    [InspectorName("建築 ID")]
    public string buildingId;
    [InspectorName("顯示名稱")] public string displayName;
    [InspectorName("建築類型")] public BuildingCategory category;
    public Sprite icon;
    public bool isEnabled = true;
    public Color markerColor = Color.white;
    [Header("輸入")]
    public ResourceAmount[] inputResources;

    [Header("輸出")]
    public ResourceAmount[] outputResources;

    public GameTimeInterval processInterval;


    [Header("成本")]
    public ResourceAmount[] constructionCosts;
    public ResourceAmount[] maintenanceCosts;

    [Header("生產")]
    public ResourceDefinition producedResource;
    public float workSpeedMultiplier = 1f;

    [Header("庫存")]
    public bool acceptsAnyResource;

}
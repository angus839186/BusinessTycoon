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
    "buildingId",
    DisplayName = "Buildings",
    ImportOrder = 200
)]
[GameDataColumnAlias(
    "producedResourceId",
    "producedResource"
)]
[GameDataColumnAlias(
    "saleResourceId",
    "saleResource"
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
    [GameDataColumn]
    [InspectorName("建築 ID")]
    public string buildingId;

    [GameDataColumn]
    [InspectorName("顯示名稱")]
    public string displayName;

    [GameDataColumn]
    [InspectorName("建築類型")]
    public BuildingCategory category;

    [GameDataColumn(AllowEmpty = true)]
    public Sprite icon;

    [GameDataColumn]
    public bool isEnabled = true;

    [GameDataColumn]
    public Color markerColor = Color.white;

    [Header("生產時間")]
    public GameTimeInterval processInterval;

    [Header("成本")]
    public ResourceAmount[] constructionCosts;
    public ResourceAmount[] maintenanceCosts;


    [Header("生產")]
    [GameDataColumn(AllowEmpty = true)]
    public ResourceDefinition producedResource;

    [GameDataColumn(MinValue = 0)]
    public float workSpeedMultiplier = 1f;

    [Header("Commercial")]
    [GameDataColumn(AllowEmpty = true)]
    public ResourceDefinition saleResource;

    [GameDataColumn(MinValue = 0)]
    public int saleAmount = 1;

    [Header("庫存")]
    [GameDataColumn]
    public bool acceptsAnyResource;

}
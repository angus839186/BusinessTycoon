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
    menuName = "BusinessTycoon/Building Definition"
)]
public sealed class BuildingDefinition : ScriptableObject
{
    [GameDataColumn]
    [Header("建築 ID")]
    public string buildingId;

    [GameDataColumn]
    [Header("顯示名稱")]
    public string displayName;

    [GameDataColumn]
    [Header("建築類型")]
    public BuildingCategory category;

    [Header("建築Icon")]
    [GameDataColumn(AllowEmpty = true)]
    public Sprite icon;

    [Header("預設是否啟用")]
    [GameDataColumn]
    public bool isEnabled = true;

    [Header("地圖標記顏色")]
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

    [Header("生產速度係數")]
    [GameDataColumn(MinValue = 0)]
    public float workSpeedMultiplier = 1f;

    [Header("出售獲得資源")]
    [GameDataColumn(AllowEmpty = true)]
    public ResourceDefinition saleResource;

    [InspectorName("單次出售數量")]
    [GameDataColumn(MinValue = 0)]
    public int saleAmount = 1;

    [Header("是否接受任何資源儲存")]
    [GameDataColumn]
    public bool acceptsAnyResource;

}
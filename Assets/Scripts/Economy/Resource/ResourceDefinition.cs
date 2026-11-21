using UnityEngine;

[GameDataTable(
    "Resources.tsv",
    "Assets/Resources/Resource",
    "resourceId",
    DisplayName = "Resources",
    ImportOrder = 0
)]
[GameDataColumnAlias("iconPath", "icon")]
[CreateAssetMenu(
    fileName = "ResourceDefinition",
    menuName = "BusinessTycoon/Resource Definition"
)]
public sealed class ResourceDefinition : ScriptableObject
{
    [Header("資源ID")]
    [GameDataColumn]
    public string resourceId;

    [Header("顯示名稱")]
    [GameDataColumn]
    public string displayName;

    [Header("資源Icon")]
    [GameDataColumn(AllowEmpty = true)]
    public Sprite icon;


    [Header("基本價格")]
    [GameDataColumn(MinValue = 0)]
    public int basePrice = 0;

    [Header("配方")]
    public ResourceRecipe recipe;
}
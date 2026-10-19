using UnityEngine;

[GameDataTable(
    "Resources.tsv",
    "Assets/Resources/Resource",
    "resourceId",
    DisplayName = "Resources",
    ImportOrder = 0
)]
[GameDataColumnAlias("iconPath", "icon")]
[GameDataColumnAlias("enabled", "isEnabled")]
[CreateAssetMenu(
    fileName = "ResourceDefinition",
    menuName = "WorldMap/Resource Definition"
)]
public sealed class ResourceDefinition : ScriptableObject
{
    [GameDataColumn]
    public string resourceId;

    [GameDataColumn]
    public string displayName;

    [GameDataColumn(AllowEmpty = true)]
    public Sprite icon;

    [GameDataColumn]
    public bool isEnabled = true;

    [GameDataColumn(MinValue = 0)]
    public int basePrice = 0;

    [Header("Production")]
    public ResourceRecipe recipe;
}
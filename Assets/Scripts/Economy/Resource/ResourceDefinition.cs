using UnityEngine;

[GameDataTable(
    "Resources.tsv",
    "Assets/Resources/Resource",
    "resourceId"
)]
[GameDataColumnAlias("iconPath", "icon")]
[GameDataColumnAlias("enabled", "isEnabled")]
[CreateAssetMenu(
    fileName = "ResourceDefinition",
    menuName = "WorldMap/Resource Definition"
)]
public sealed class ResourceDefinition : ScriptableObject
{
    public string resourceId;
    public string displayName;
    public Sprite icon;
    public bool isEnabled = true;

    public int basePrice = 0;

    [Header("Production")]
    public ResourceRecipe recipe;
}
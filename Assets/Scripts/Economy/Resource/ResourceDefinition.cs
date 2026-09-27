using UnityEngine;

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
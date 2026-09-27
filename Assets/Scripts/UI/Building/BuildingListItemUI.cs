using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuildingListItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI categoryText;

    public void Initialize(BuildingInstance building)
    {
        if (building == null || building.Definition == null)
            return;

        BuildingDefinition definition = building.Definition;

        if (nameText != null)
            nameText.text = definition.displayName;

        if (categoryText != null)
            categoryText.text = GetCategoryText(definition.category);

        if (iconImage != null)
        {
            iconImage.sprite = definition.icon;
            iconImage.enabled = definition.icon != null;
        }
    }

    private static string GetCategoryText(BuildingCategory category)
    {
        return category switch
        {
            BuildingCategory.Resource => "資源",
            BuildingCategory.Processing => "加工",
            BuildingCategory.Commercial => "商業",
            BuildingCategory.Storage => "倉庫",
            _ => category.ToString()
        };
    }
}
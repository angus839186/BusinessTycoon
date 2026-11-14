using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class BuildingListItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private Button selectButton;

    private BuildingInstance building;
    private Action<BuildingInstance> selectedCallback;
    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(
                HandleSelectClicked
            );
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(
                HandleSelectClicked
            );
        }
    }

    public void Initialize(BuildingInstance targetBuilding, Action<BuildingInstance> onSelected)
    {
        building = targetBuilding;
        selectedCallback = onSelected;

        if (building == null || building.Definition == null) return;

        BuildingDefinition definition = building.Definition;

        if (nameText != null) nameText.text = definition.displayName;

        if (categoryText != null)
            categoryText.text = GetCategoryText(definition.category);

        if (iconImage != null)
        {
            iconImage.sprite = definition.icon;
            iconImage.enabled = definition.icon != null;
        }
    }

    private void HandleSelectClicked()
    {
        if (building == null) return;
        selectedCallback?.Invoke(building);
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
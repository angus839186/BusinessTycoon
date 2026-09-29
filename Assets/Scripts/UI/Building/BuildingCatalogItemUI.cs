using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class BuildingCatalogItemUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject selectedIndicator;

    private BuildingDefinition definition;
    private Action<BuildingDefinition> selectedCallback;

    private void Awake()
    {
        if (selectButton == null)
        {
            selectButton = GetComponent<Button>();
        }

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Initialize(
        BuildingDefinition building,
        Action<BuildingDefinition> onSelected
    )
    {
        definition = building;
        selectedCallback = onSelected;

        if (nameText != null)
        {
            nameText.text =
                definition != null &&
                !string.IsNullOrWhiteSpace(definition.displayName)
                    ? definition.displayName
                    : definition != null
                        ? definition.buildingId
                        : "";
        }

        if (categoryText != null)
        {
            categoryText.text =
                definition != null
                    ? FormatBuildingSummary(definition)
                    : "";
        }

        if (costText != null)
        {
            costText.text =
                definition != null
                    ? $"基礎成本：{FormatResourceAmounts(definition.constructionCosts)}"
                    : "";
        }

        if (iconImage != null)
        {
            iconImage.sprite =
                definition != null
                    ? definition.icon
                    : null;

            iconImage.enabled =
                iconImage.sprite != null;
        }

        if (selectButton != null)
        {
            selectButton.interactable =
                definition != null &&
                definition.isEnabled;
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }

    private void HandleClicked()
    {
        if (definition == null ||
            !definition.isEnabled)
        {
            return;
        }

        selectedCallback?.Invoke(definition);
    }

    private static string FormatBuildingSummary(
    BuildingDefinition building
)
    {
        string category = GetCategoryText(building.category);
        double cycleMinutes = GetBaseCycleMinutes(building);

        if (cycleMinutes <= 0)
        {
            return category;
        }

        return $"{category} · 運作週期 {FormatDuration(cycleMinutes)}";
    }

    private static double GetBaseCycleMinutes(
        BuildingDefinition building
    )
    {
        double minutes = building.processInterval.ToMinutes();

        if (minutes <= 0)
        {
            return 0;
        }

        float speed = Mathf.Max(
            0.01f,
            building.workSpeedMultiplier
        );

        return minutes / speed;
    }

    private static string FormatResourceAmounts(
        ResourceAmount[] resourceAmounts
    )
    {
        if (resourceAmounts == null ||
            resourceAmounts.Length == 0)
        {
            return "免費";
        }

        List<string> parts = new List<string>();

        foreach (ResourceAmount resourceAmount in resourceAmounts)
        {
            if (resourceAmount == null ||
                resourceAmount.resource == null ||
                resourceAmount.amount <= 0)
            {
                continue;
            }

            string resourceName =
                string.IsNullOrWhiteSpace(
                    resourceAmount.resource.displayName
                )
                    ? resourceAmount.resource.resourceId
                    : resourceAmount.resource.displayName;

            parts.Add(
                $"{resourceName} x{resourceAmount.amount}"
            );
        }

        return parts.Count > 0
            ? string.Join("、", parts)
            : "免費";
    }

    private static string FormatDuration(double minutes)
    {
        long remainingMinutes =
            (long)Math.Ceiling(minutes);

        const long minutesPerHour = 60;
        const long minutesPerDay = 24 * minutesPerHour;
        const long minutesPerMonth = 30 * minutesPerDay;

        long months = remainingMinutes / minutesPerMonth;
        remainingMinutes %= minutesPerMonth;

        long days = remainingMinutes / minutesPerDay;
        remainingMinutes %= minutesPerDay;

        long hours = remainingMinutes / minutesPerHour;
        long remaining = remainingMinutes % minutesPerHour;

        List<string> parts = new List<string>();

        if (months > 0)
            parts.Add($"{months}月");

        if (days > 0)
            parts.Add($"{days}日");

        if (hours > 0)
            parts.Add($"{hours}時");

        if (remaining > 0 || parts.Count == 0)
            parts.Add($"{remaining}分");

        return string.Join("", parts);
    }

    private static string GetCategoryText(
        BuildingCategory category
    )
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
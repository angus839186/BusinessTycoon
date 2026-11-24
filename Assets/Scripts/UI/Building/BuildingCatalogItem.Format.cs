using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BuildingCatalogItem
{
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
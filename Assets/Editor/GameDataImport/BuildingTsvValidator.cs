using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class BuildingTsvValidator
{
    private const string ExpectedHeader =
        "buildingId\tdisplayName\tcategory\tproducedResourceId\t" +
        "processMonths\tprocessDays\tprocessHours\tprocessMinutes\t" +
        "workSpeedMultiplier\tacceptsAnyResource\tmarkerColor\t" +
        "iconPath\tenabled";

    public static void Validate(
        string path,
        IReadOnlyList<ResourceImportRow> resourceRows,
        out List<BuildingImportRow> rows,
        out List<string> errors
    )
    {
        rows = new List<BuildingImportRow>();
        errors = new List<string>();

        string fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            errors.Add($"找不到檔案：{path}");
            return;
        }

        string[] lines;

        try
        {
            lines = File.ReadAllLines(
                fullPath,
                new UTF8Encoding(false, true)
            );
        }
        catch (Exception exception)
        {
            errors.Add($"讀取 Buildings TSV 失敗：{exception.Message}");
            return;
        }

        if (lines.Length == 0 ||
            lines[0].TrimStart('\uFEFF') != ExpectedHeader)
        {
            errors.Add("Buildings TSV 標頭錯誤。");
            return;
        }

        HashSet<string> resourceIds =
            new HashSet<string>(StringComparer.Ordinal);

        foreach (ResourceImportRow resourceRow in resourceRows)
            resourceIds.Add(resourceRow.ResourceId);

        HashSet<string> buildingIds =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
                continue;

            int lineNumber = index + 1;
            string[] cells = lines[index].Split('\t');

            if (cells.Length != 13)
            {
                errors.Add($"第 {lineNumber} 行應有 13 欄。");
                continue;
            }

            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                cells[cellIndex] = cells[cellIndex].Trim();

            bool valid = true;
            string buildingId = cells[0];
            string displayName = cells[1];
            string producedResourceId = cells[3];

            if (string.IsNullOrWhiteSpace(buildingId) ||
                !buildingIds.Add(buildingId))
            {
                errors.Add($"第 {lineNumber} 行 buildingId 空白或重複。");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add($"第 {lineNumber} 行 displayName 不可空白。");
                valid = false;
            }

            bool categoryValid = TryParseCategory(
                cells[2],
                out BuildingCategory category
            );

            if (!categoryValid)
            {
                errors.Add($"第 {lineNumber} 行 category 無效。");
                valid = false;
            }

            bool monthsValid =
    TryParseNonNegative(cells[4], out int months);

            bool daysValid =
                TryParseNonNegative(cells[5], out int days);

            bool hoursValid =
                TryParseNonNegative(cells[6], out int hours);

            bool minutesValid =
                TryParseNonNegative(cells[7], out int minutes);

            bool timeValid =
                monthsValid &&
                daysValid &&
                hoursValid &&
                minutesValid;

            if (!timeValid)
            {
                errors.Add($"第 {lineNumber} 行時間必須是非負整數。");
                valid = false;
            }

            bool speedValid = float.TryParse(
                cells[8],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float speed
            ) && speed > 0f;

            if (!speedValid)
            {
                errors.Add($"第 {lineNumber} 行速度倍率必須大於 0。");
                valid = false;
            }

            if (!bool.TryParse(cells[9], out bool acceptsAnyResource))
            {
                errors.Add($"第 {lineNumber} 行 acceptsAnyResource 無效。");
                valid = false;
            }

            if (!ColorUtility.TryParseHtmlString(
                cells[10],
                out Color markerColor
            ))
            {
                errors.Add($"第 {lineNumber} 行 markerColor 無效。");
                valid = false;
            }

            if (!bool.TryParse(cells[12], out bool enabled))
            {
                errors.Add($"第 {lineNumber} 行 enabled 無效。");
                valid = false;
            }

            if (categoryValid)
            {
                bool needsProducedResource =
                    category == BuildingCategory.Resource ||
                    category == BuildingCategory.Processing;

                if (needsProducedResource &&
                    !resourceIds.Contains(producedResourceId))
                {
                    errors.Add(
                        $"第 {lineNumber} 行找不到 producedResourceId：" +
                        producedResourceId
                    );

                    valid = false;
                }

                if (!needsProducedResource &&
                    !string.IsNullOrWhiteSpace(producedResourceId))
                {
                    errors.Add(
                        $"第 {lineNumber} 行此分類不應設定 producedResourceId。"
                    );

                    valid = false;
                }

                if (category == BuildingCategory.Storage &&
                    !acceptsAnyResource)
                {
                    errors.Add($"第 {lineNumber} 行倉庫必須接受所有資源。");
                    valid = false;
                }
            }

            GameTimeInterval interval = new GameTimeInterval
            {
                months = months,
                days = days,
                hours = hours,
                minutes = minutes
            };

            if (categoryValid &&
                category != BuildingCategory.Storage &&
                timeValid &&
                interval.ToMinutes() <= 0)
            {
                errors.Add($"第 {lineNumber} 行生產週期必須大於 0。");
                valid = false;
            }

            if (!valid)
                continue;

            rows.Add(new BuildingImportRow(
                lineNumber,
                buildingId,
                displayName,
                category,
                producedResourceId,
                interval,
                speed,
                acceptsAnyResource,
                markerColor,
                cells[11],
                enabled
            ));
        }
    }

    private static bool TryParseNonNegative(
        string value,
        out int result
    )
    {
        return int.TryParse(value, out result) && result >= 0;
    }

    private static bool TryParseCategory(
        string value,
        out BuildingCategory category
    )
    {
        return Enum.TryParse(value, false, out category) &&
               Enum.IsDefined(typeof(BuildingCategory), category);
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public static class RecipeTsvValidator
{
    private const string ExpectedHeader =
        "ownerResourceId\tioType\tresourceId\tamount\tenabled";

    public static void Validate(
        string projectRelativePath,
        IReadOnlyList<ResourceImportRow> resourceRows,
        out List<RecipeImportRow> rows,
        out List<string> errors
    )
    {
        rows = new List<RecipeImportRow>();
        errors = new List<string>();

        string fullPath = Path.GetFullPath(projectRelativePath);

        if (!File.Exists(fullPath))
        {
            errors.Add($"找不到檔案：{projectRelativePath}");
            return;
        }

        string[] lines;

        try
        {
            UTF8Encoding utf8 = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true
            );

            lines = File.ReadAllLines(fullPath, utf8);
        }
        catch (Exception exception)
        {
            errors.Add($"讀取 Recipes TSV 失敗：{exception.Message}");
            return;
        }

        if (lines.Length == 0)
        {
            errors.Add("Recipes TSV 是空檔案。");
            return;
        }

        string actualHeader = lines[0].TrimStart('\uFEFF');

        if (!string.Equals(
            actualHeader,
            ExpectedHeader,
            StringComparison.Ordinal
        ))
        {
            errors.Add(
                "Recipes TSV 標頭錯誤，預期：\n" +
                ExpectedHeader
            );

            return;
        }

        HashSet<string> knownResourceIds =
            BuildKnownResourceIds(resourceRows);

        HashSet<string> recipeKeys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 1; index < lines.Length; index++)
        {
            int lineNumber = index + 1;
            string line = lines[index];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cells = line.Split('\t');

            if (cells.Length != 5)
            {
                errors.Add(
                    $"第 {lineNumber} 行：應有 5 欄，" +
                    $"實際為 {cells.Length} 欄。"
                );

                continue;
            }

            string ownerResourceId = cells[0].Trim();
            string ioTypeText = cells[1].Trim();
            string resourceId = cells[2].Trim();
            string amountText = cells[3].Trim();
            string enabledText = cells[4].Trim();

            bool rowIsValid = true;

            if (!knownResourceIds.Contains(ownerResourceId))
            {
                errors.Add(
                    $"第 {lineNumber} 行：" +
                    $"找不到 ownerResourceId：{ownerResourceId}"
                );

                rowIsValid = false;
            }

            if (!knownResourceIds.Contains(resourceId))
            {
                errors.Add(
                    $"第 {lineNumber} 行：" +
                    $"找不到 resourceId：{resourceId}"
                );

                rowIsValid = false;
            }

            bool ioTypeIsValid = TryParseIoType(
                ioTypeText,
                out RecipeIoType ioType
            );

            if (!ioTypeIsValid)
            {
                errors.Add(
                    $"第 {lineNumber} 行：" +
                    "ioType 必須是 Input 或 Output。"
                );

                rowIsValid = false;
            }

            bool amountIsValid = int.TryParse(
                amountText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int amount
            );

            if (!amountIsValid || amount <= 0)
            {
                errors.Add(
                    $"第 {lineNumber} 行：" +
                    "amount 必須是大於 0 的整數。"
                );

                rowIsValid = false;
            }

            if (!bool.TryParse(enabledText, out bool enabled))
            {
                errors.Add(
                    $"第 {lineNumber} 行：" +
                    "enabled 必須是 TRUE 或 FALSE。"
                );

                rowIsValid = false;
            }

            if (ioTypeIsValid &&
                !string.IsNullOrWhiteSpace(ownerResourceId) &&
                !string.IsNullOrWhiteSpace(resourceId))
            {
                string recipeKey =
                    $"{ownerResourceId}\t{ioType}\t{resourceId}";

                if (!recipeKeys.Add(recipeKey))
                {
                    errors.Add(
                        $"第 {lineNumber} 行：配方重複：" +
                        $"{ownerResourceId} / {ioType} / {resourceId}"
                    );

                    rowIsValid = false;
                }
            }

            if (!rowIsValid)
                continue;

            rows.Add(
                new RecipeImportRow(
                    lineNumber,
                    ownerResourceId,
                    ioType,
                    resourceId,
                    amount,
                    enabled
                )
            );
        }
    }

    private static HashSet<string> BuildKnownResourceIds(
        IReadOnlyList<ResourceImportRow> resourceRows
    )
    {
        HashSet<string> resourceIds =
            new HashSet<string>(StringComparer.Ordinal);

        if (resourceRows == null)
            return resourceIds;

        foreach (ResourceImportRow row in resourceRows)
        {
            if (row == null ||
                string.IsNullOrWhiteSpace(row.ResourceId))
            {
                continue;
            }

            resourceIds.Add(row.ResourceId);
        }

        return resourceIds;
    }

    private static bool TryParseIoType(
        string value,
        out RecipeIoType ioType
    )
    {
        if (value == "Input")
        {
            ioType = RecipeIoType.Input;
            return true;
        }

        if (value == "Output")
        {
            ioType = RecipeIoType.Output;
            return true;
        }

        ioType = default;
        return false;
    }
}
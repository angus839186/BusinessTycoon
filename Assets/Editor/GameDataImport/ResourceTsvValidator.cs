using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public static class ResourceTsvValidator
{
    private static readonly string[] ExpectedHeaders =
    {
        "resourceId",
        "displayName",
        "basePrice",
        "iconPath",
        "enabled"
    };

    public static void Validate(
        string projectRelativePath,
        out List<ResourceImportRow> rows,
        out List<string> errors
    )
    {
        rows = new List<ResourceImportRow>();
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
            errors.Add($"讀取檔案失敗：{exception.Message}");
            return;
        }

        if (lines.Length == 0)
        {
            errors.Add("TSV 是空檔案。");
            return;
        }

        if (!ValidateHeaders(lines[0], errors))
            return;

        HashSet<string> resourceIds =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 1; index < lines.Length; index++)
        {
            int lineNumber = index + 1;
            string line = lines[index];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cells = line.Split('\t');

            if (cells.Length != ExpectedHeaders.Length)
            {
                errors.Add(
                    $"第 {lineNumber} 行：應有 {ExpectedHeaders.Length} 欄，" +
                    $"實際為 {cells.Length} 欄。"
                );

                continue;
            }

            string resourceId = cells[0].Trim();
            string displayName = cells[1].Trim();
            string basePriceText = cells[2].Trim();
            string iconPath = cells[3].Trim();
            string enabledText = cells[4].Trim();

            bool rowIsValid = true;

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                errors.Add($"第 {lineNumber} 行：resourceId 不可空白。");
                rowIsValid = false;
            }
            else if (!resourceIds.Add(resourceId))
            {
                errors.Add(
                    $"第 {lineNumber} 行：resourceId 重複：{resourceId}"
                );

                rowIsValid = false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add($"第 {lineNumber} 行：displayName 不可空白。");
                rowIsValid = false;
            }

            bool priceIsValid = int.TryParse(
                basePriceText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int basePrice
            );

            if (!priceIsValid || basePrice < 0)
            {
                errors.Add(
                    $"第 {lineNumber} 行：basePrice 必須是大於或等於 0 的整數。"
                );

                rowIsValid = false;
            }

            if (!bool.TryParse(enabledText, out bool enabled))
            {
                errors.Add(
                    $"第 {lineNumber} 行：enabled 必須是 TRUE 或 FALSE。"
                );

                rowIsValid = false;
            }

            if (!rowIsValid)
                continue;

            rows.Add(
                new ResourceImportRow(
                    lineNumber,
                    resourceId,
                    displayName,
                    basePrice,
                    iconPath,
                    enabled
                )
            );
        }
    }

    private static bool ValidateHeaders(
        string headerLine,
        List<string> errors
    )
    {
        string[] headers = headerLine.Split('\t');

        if (headers.Length != ExpectedHeaders.Length)
        {
            errors.Add(
                $"標頭應有 {ExpectedHeaders.Length} 欄，" +
                $"實際為 {headers.Length} 欄。"
            );

            return false;
        }

        bool isValid = true;

        for (int index = 0; index < ExpectedHeaders.Length; index++)
        {
            string actualHeader = headers[index]
                .Trim()
                .TrimStart('\uFEFF');

            string expectedHeader = ExpectedHeaders[index];

            if (string.Equals(
                actualHeader,
                expectedHeader,
                StringComparison.Ordinal
            ))
            {
                continue;
            }

            errors.Add(
                $"第 {index + 1} 欄標頭錯誤：" +
                $"預期 {expectedHeader}，實際 {actualHeader}。"
            );

            isValid = false;
        }

        return isValid;
    }
}
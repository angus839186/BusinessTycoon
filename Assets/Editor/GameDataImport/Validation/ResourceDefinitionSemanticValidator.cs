using System;
using System.Collections.Generic;
using System.Globalization;

public class ResourceDefinitionSemanticValidator :
    IGameDataSemanticValidator
{
    private static readonly string[] RequiredFieldPaths =
    {
        "displayName",
        "basePrice",
        "icon",
        "isEnabled"
    };

    public Type DefinitionType =>
        typeof(ResourceDefinition);

    public void Validate(
        GameDataTableSchema schema,
        GameDataTsvTable table,
        GameDataValidationResult result
    )
    {
        Dictionary<string, string> headers =
            BuildHeaderMap(schema, table);

        foreach (string fieldPath in RequiredFieldPaths)
        {
            if (!headers.ContainsKey(fieldPath))
            {
                result.AddError(
                    0,
                    fieldPath,
                    "Required column was not found."
                );
            }
        }

        if (!result.IsValid)
        {
            return;
        }

        foreach (GameDataTsvRow row in table.Rows)
        {
            ValidateRow(row, headers, result);
        }
    }

    private static void ValidateRow(
        GameDataTsvRow row,
        Dictionary<string, string> headers,
        GameDataValidationResult result
    )
    {
        string displayName =
            GetValue(row, headers, "displayName");

        if (string.IsNullOrWhiteSpace(displayName))
        {
            result.AddError(
                row.LineNumber,
                headers["displayName"],
                "Display name cannot be empty."
            );
        }

        int basePrice = int.Parse(
            GetValue(row, headers, "basePrice"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture
        );

        if (basePrice < 0)
        {
            result.AddError(
                row.LineNumber,
                headers["basePrice"],
                "Base price cannot be negative."
            );
        }
    }

    private static Dictionary<string, string> BuildHeaderMap(
        GameDataTableSchema schema,
        GameDataTsvTable table
    )
    {
        Dictionary<string, string> result =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (string header in table.Headers)
        {
            result[schema.ResolveFieldPath(header)] =
                header;
        }

        return result;
    }

    private static string GetValue(
        GameDataTsvRow row,
        Dictionary<string, string> headers,
        string fieldPath
    )
    {
        return row.GetValue(headers[fieldPath]);
    }
}
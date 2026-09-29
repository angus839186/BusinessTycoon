using System;
using System.Collections.Generic;
using System.Globalization;

public class BuildingDefinitionSemanticValidator :
    IGameDataSemanticValidator
{
    private static readonly string[] RequiredFieldPaths =
    {
        "displayName",
        "category",
        "producedResource",
        "processInterval.months",
        "processInterval.days",
        "processInterval.hours",
        "processInterval.minutes",
        "workSpeedMultiplier",
        "acceptsAnyResource",
        "markerColor",
        "icon",
        "isEnabled"
    };

    public Type DefinitionType =>
        typeof(BuildingDefinition);

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

        Enum.TryParse(
            GetValue(row, headers, "category"),
            true,
            out BuildingCategory category
        );

        string producedResourceId =
            GetValue(row, headers, "producedResource");

        int months = ParseInt(
            row,
            headers,
            "processInterval.months"
        );

        int days = ParseInt(
            row,
            headers,
            "processInterval.days"
        );

        int hours = ParseInt(
            row,
            headers,
            "processInterval.hours"
        );

        int minutes = ParseInt(
            row,
            headers,
            "processInterval.minutes"
        );

        bool timeIsNonNegative =
            months >= 0 &&
            days >= 0 &&
            hours >= 0 &&
            minutes >= 0;

        if (!timeIsNonNegative)
        {
            result.AddError(
                row.LineNumber,
                headers["processInterval.months"],
                "Production time values cannot be negative."
            );
        }

        float speed = float.Parse(
            GetValue(
                row,
                headers,
                "workSpeedMultiplier"
            ),
            CultureInfo.InvariantCulture
        );

        if (speed <= 0f)
        {
            result.AddError(
                row.LineNumber,
                headers["workSpeedMultiplier"],
                "Work speed multiplier must be greater than 0."
            );
        }

        bool acceptsAnyResource = bool.Parse(
            GetValue(
                row,
                headers,
                "acceptsAnyResource"
            )
        );

        bool needsProducedResource =
            category == BuildingCategory.Resource ||
            category == BuildingCategory.Processing;

        if (needsProducedResource &&
            string.IsNullOrWhiteSpace(producedResourceId))
        {
            result.AddError(
                row.LineNumber,
                headers["producedResource"],
                "This building category requires a produced resource."
            );
        }

        if (!needsProducedResource &&
            !string.IsNullOrWhiteSpace(producedResourceId))
        {
            result.AddError(
                row.LineNumber,
                headers["producedResource"],
                "This building category cannot produce a resource."
            );
        }

        if (category == BuildingCategory.Storage &&
            !acceptsAnyResource)
        {
            result.AddError(
                row.LineNumber,
                headers["acceptsAnyResource"],
                "Storage buildings must accept any resource."
            );
        }

        GameTimeInterval interval =
            new GameTimeInterval
            {
                months = months,
                days = days,
                hours = hours,
                minutes = minutes
            };

        if (category != BuildingCategory.Storage &&
            timeIsNonNegative &&
            interval.ToMinutes() <= 0)
        {
            result.AddError(
                row.LineNumber,
                headers["processInterval.minutes"],
                "Production time must be greater than 0."
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

    private static int ParseInt(
        GameDataTsvRow row,
        Dictionary<string, string> headers,
        string fieldPath
    )
    {
        return int.Parse(
            GetValue(row, headers, fieldPath),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture
        );
    }
}
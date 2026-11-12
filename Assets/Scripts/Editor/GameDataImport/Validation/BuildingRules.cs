using System;
using System.Collections.Generic;
using System.Globalization;

public class BuildingRules :
    IDataRules
{

    public Type DefinitionType =>
        typeof(BuildingDefinition);

    public void Validate(
    TableSchema schema,
    TsvTable table,
    ValidationResult result
)
    {
        Dictionary<string, string> headers =
            BuildHeaderMap(schema, table);

        foreach (TsvRow row in table.Rows)
        {
            ValidateRow(row, headers, result);
        }
    }

    private static void ValidateRow(
        TsvRow row,
        Dictionary<string, string> headers,
        ValidationResult result
    )
    {

        Enum.TryParse(
            GetValue(row, headers, "category"),
            true,
            out BuildingCategory category
        );

        string producedResourceId =
            GetValue(row, headers, "producedResource");

        string saleResourceId =
            GetValue(row, headers, "saleResource");

        int saleAmount =
            ParseInt(row, headers, "saleAmount");

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

        bool needsSaleResource =
    category == BuildingCategory.Commercial;

        if (needsSaleResource &&
            string.IsNullOrWhiteSpace(saleResourceId))
        {
            result.AddError(
                row.LineNumber,
                headers["saleResource"],
                "Commercial buildings require a sale resource."
            );
        }

        if (needsSaleResource && saleAmount <= 0)
        {
            result.AddError(
                row.LineNumber,
                headers["saleAmount"],
                "Commercial sale amount must be greater than 0."
            );
        }

        if (!needsSaleResource &&
            !string.IsNullOrWhiteSpace(saleResourceId))
        {
            result.AddError(
                row.LineNumber,
                headers["saleResource"],
                "Only commercial buildings can define a sale resource."
            );
        }

        if (!needsSaleResource && saleAmount != 0)
        {
            result.AddError(
                row.LineNumber,
                headers["saleAmount"],
                "Non-commercial buildings must use sale amount 0."
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

        if (category != BuildingCategory.Storage && interval.ToMinutes() <= 0)
        {
            result.AddError(
                row.LineNumber,
                headers["processInterval.minutes"],
                "Production time must be greater than 0."
            );
        }
    }

    private static Dictionary<string, string> BuildHeaderMap(
        TableSchema schema,
        TsvTable table
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
        TsvRow row,
        Dictionary<string, string> headers,
        string fieldPath
    )
    {
        return row.GetValue(headers[fieldPath]);
    }

    private static int ParseInt(
        TsvRow row,
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
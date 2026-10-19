using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class TableValidator
{
    public static ValidationResult Validate(
        TableSchema schema,
        TsvTable table
    )
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        ValidationResult result =
    new ValidationResult();

        ValidateRequiredColumns(
            schema,
            table,
            result
        );

        Dictionary<string, FieldInfo> fields =
            ResolveFields(schema, table, result);

        Dictionary<string, ColumnSchema> columnsByHeader =
BuildColumnMap(schema);

        Dictionary<Type, HashSet<string>> referenceIds =
new Dictionary<Type, HashSet<string>>();

        HashSet<Type> failedReferenceTypes =
            new HashSet<Type>();

        string idHeader = FindIdHeader(schema, table);

        if (idHeader == null)
        {
            result.AddError(
                0,
                schema.IdField,
                "ID column was not found."
            );
        }

        HashSet<string> ids =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (TsvRow row in table.Rows)
        {
            ValidateValues(
    row,
    fields,
    columnsByHeader,
    idHeader,
    referenceIds,
    failedReferenceTypes,
    result
);

            if (idHeader != null)
            {
                ValidateId(row, idHeader, ids, result);
            }
        }
        if (result.IsValid)
        {
            RuleRegistry.Validate(
                schema,
                table,
                result
            );
        }
        return result;
    }
    private static Dictionary<string, ColumnSchema>
        BuildColumnMap(TableSchema schema)
    {
        Dictionary<string, ColumnSchema> result =
            new Dictionary<string, ColumnSchema>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (ColumnSchema column in schema.Columns)
        {
            result[column.Header] = column;
        }

        return result;
    }
    private static void ValidateRequiredColumns(
    TableSchema schema,
    TsvTable table,
    ValidationResult result
)
    {
        foreach (ColumnSchema column in schema.Columns)
        {
            if (!column.Required)
            {
                continue;
            }

            if (HasHeader(table, column.Header))
            {
                continue;
            }

            result.AddError(
                0,
                column.Header,
                $"Required column '{column.Header}' was not found."
            );
        }
    }

    private static bool HasHeader(
        TsvTable table,
        string expectedHeader
    )
    {
        foreach (string header in table.Headers)
        {
            if (string.Equals(
                header,
                expectedHeader,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, FieldInfo> ResolveFields(
        TableSchema schema,
        TsvTable table,
        ValidationResult result
    )
    {
        Dictionary<string, FieldInfo> fields =
            new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
        HashSet<string> allowedFieldPaths = new HashSet<string>
        (StringComparer.OrdinalIgnoreCase);

        foreach (ColumnSchema column in schema.Columns)
        {
            allowedFieldPaths.Add(
                column.FieldPath
            );
        }

        foreach (string header in table.Headers)
        {
            string fieldPath =
                schema.ResolveFieldPath(header);

            if (!allowedFieldPaths.Contains(fieldPath))
            {
                result.AddError(
                    0,
                    header,
                    $"Column '{header}' is not importable. " +
                    $"Field '{fieldPath}' must have " +
                    $"GameDataColumnAttribute."
                );

                continue;
            }

            if (!FieldResolver.TryResolve(
                schema.DefinitionType,
                fieldPath,
                out FieldInfo field,
                out string error
            ))
            {
                result.AddError(0, header, error);
                continue;
            }

            fields.Add(header, field);
        }

        return fields;
    }

    private static string FindIdHeader(
    TableSchema schema,
    TsvTable table
)
    {
        foreach (string header in table.Headers)
        {
            string fieldPath =
                schema.ResolveFieldPath(header);

            if (string.Equals(
                fieldPath,
                schema.IdField,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return header;
            }
        }

        return null;
    }

    private static void ValidateValues(
    TsvRow row,
    Dictionary<string, FieldInfo> fields,
    Dictionary<string, ColumnSchema> columnsByHeader,
    string idHeader,
    Dictionary<Type, HashSet<string>> referenceIds,
    HashSet<Type> failedReferenceTypes,
    ValidationResult result
)
    {
        foreach (
            KeyValuePair<string, FieldInfo> entry
            in fields
        )
        {
            string header = entry.Key;
            string rawValue = row.GetValue(header);
            Type fieldType = entry.Value.FieldType;

            columnsByHeader.TryGetValue(
                header,
                out ColumnSchema column
            );

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                bool isIdColumn =
                    string.Equals(
                        header,
                        idHeader,
                        StringComparison.OrdinalIgnoreCase
                    );

                if (column != null &&
                    !column.AllowEmpty &&
                    !isIdColumn)
                {
                    result.AddError(
                        row.LineNumber,
                        header,
                        "Value cannot be empty."
                    );
                }

                // ID 空值由 ValidateId 負責，避免重複錯誤。
                // 允許空值的欄位也不需要再做型別轉換。
                if (column != null || isIdColumn)
                {
                    continue;
                }
            }

            if (
                typeof(ScriptableObject)
                    .IsAssignableFrom(fieldType)
            )
            {
                ValidateReferenceId(
                    row,
                    header,
                    rawValue,
                    fieldType,
                    referenceIds,
                    failedReferenceTypes,
                    result
                );

                continue;
            }

            if (!ValueParser.TryConvert(
                rawValue,
                fieldType,
                out object convertedValue,
                out string error
            ))
            {
                result.AddError(
                    row.LineNumber,
                    header,
                    error
                );

                continue;
            }

            if (column != null)
            {
                ValidateRange(
                    row,
                    column,
                    convertedValue,
                    result
                );
            }
        }
    }
    private static void ValidateRange(
        TsvRow row,
        ColumnSchema column,
        object convertedValue,
        ValidationResult result
    )
    {
        if (!column.HasMinValue &&
            !column.HasMaxValue)
        {
            return;
        }

        if (!IsNumericType(column.FieldType))
        {
            result.AddError(
                0,
                column.Header,
                $"MinValue and MaxValue can only be used " +
                $"on numeric fields. Field: {column.FieldPath}"
            );

            return;
        }

        double numericValue =
            Convert.ToDouble(convertedValue);

        if (column.HasMinValue &&
            numericValue < column.MinValue)
        {
            result.AddError(
                row.LineNumber,
                column.Header,
                $"Value must be greater than or equal to " +
                $"{column.MinValue:0.###}."
            );
        }

        if (column.HasMaxValue &&
            numericValue > column.MaxValue)
        {
            result.AddError(
                row.LineNumber,
                column.Header,
                $"Value must be less than or equal to " +
                $"{column.MaxValue:0.###}."
            );
        }
    }

    private static bool IsNumericType(Type type)
    {
        return
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal);
    }
    private static void ValidateReferenceId(
    TsvRow row,
    string header,
    string rawValue,
    Type referenceType,
    Dictionary<Type, HashSet<string>> referenceIds,
    HashSet<Type> failedReferenceTypes,
    ValidationResult result
)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return;
        }

        if (failedReferenceTypes.Contains(referenceType))
        {
            return;
        }

        if (!referenceIds.TryGetValue(
            referenceType,
            out HashSet<string> ids
        ))
        {
            if (!TableIdProvider.TryLoadIds(
                referenceType,
                out ids,
                out List<string> errors
            ))
            {
                failedReferenceTypes.Add(referenceType);

                foreach (string error in errors)
                {
                    result.AddError(
                        0,
                        header,
                        $"Cannot validate {referenceType.Name}: {error}"
                    );
                }

                return;
            }

            referenceIds.Add(referenceType, ids);
        }

        if (!ids.Contains(rawValue))
        {
            result.AddError(
                row.LineNumber,
                header,
                $"{referenceType.Name} ID '{rawValue}' " +
                "was not found in its TSV source."
            );
        }
    }

    private static void ValidateId(
        TsvRow row,
        string idHeader,
        HashSet<string> ids,
        ValidationResult result
    )
    {
        string id = row.GetValue(idHeader);

        if (string.IsNullOrWhiteSpace(id))
        {
            result.AddError(
                row.LineNumber,
                idHeader,
                "ID cannot be empty."
            );

            return;
        }

        if (!ids.Add(id))
        {
            result.AddError(
                row.LineNumber,
                idHeader,
                $"Duplicate ID '{id}'."
            );
        }
    }
}
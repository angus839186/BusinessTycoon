using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class GameDataTableValidator
{
    public static GameDataValidationResult Validate(
        GameDataTableSchema schema,
        GameDataTsvTable table
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

        GameDataValidationResult result =
            new GameDataValidationResult();

        Dictionary<string, FieldInfo> fields =
            ResolveFields(schema, table, result);

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

        foreach (GameDataTsvRow row in table.Rows)
        {
            ValidateValues(
    row,
    fields,
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
            GameDataSemanticValidatorRegistry.Validate(
                schema,
                table,
                result
            );
        }
        return result;
    }

    private static Dictionary<string, FieldInfo> ResolveFields(
        GameDataTableSchema schema,
        GameDataTsvTable table,
        GameDataValidationResult result
    )
    {
        Dictionary<string, FieldInfo> fields =
            new Dictionary<string, FieldInfo>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (string header in table.Headers)
        {
            string fieldPath =
                schema.ResolveFieldPath(header);

            if (!GameDataFieldPathResolver.TryResolve(
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
        GameDataTableSchema schema,
        GameDataTsvTable table
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
    GameDataTsvRow row,
    Dictionary<string, FieldInfo> fields,
    Dictionary<Type, HashSet<string>> referenceIds,
    HashSet<Type> failedReferenceTypes,
    GameDataValidationResult result
)
    {
        foreach (KeyValuePair<string, FieldInfo> entry in fields)
        {
            string rawValue = row.GetValue(entry.Key);
            Type fieldType = entry.Value.FieldType;

            if (typeof(ScriptableObject).IsAssignableFrom(fieldType))
            {
                ValidateReferenceId(
                    row,
                    entry.Key,
                    rawValue,
                    fieldType,
                    referenceIds,
                    failedReferenceTypes,
                    result
                );

                continue;
            }

            if (!GameDataValueConverter.TryConvert(
                rawValue,
                fieldType,
                out _,
                out string error
            ))
            {
                result.AddError(
                    row.LineNumber,
                    entry.Key,
                    error
                );
            }
        }
    }

    private static void ValidateReferenceId(
    GameDataTsvRow row,
    string header,
    string rawValue,
    Type referenceType,
    Dictionary<Type, HashSet<string>> referenceIds,
    HashSet<Type> failedReferenceTypes,
    GameDataValidationResult result
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
            if (!GameDataTableIdProvider.TryLoadIds(
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
        GameDataTsvRow row,
        string idHeader,
        HashSet<string> ids,
        GameDataValidationResult result
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
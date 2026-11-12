using System;
using System.Collections.Generic;

public static class TableIdProvider
{
    public static bool TryLoadIds(
        Type definitionType,
        out HashSet<string> ids,
        out List<string> errors
    )
    {
        ids = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        );

        errors = new List<string>();

        try
        {
            TableSchema schema =
                TableSchema.Create(definitionType);

            string sourcePath =
                $"Assets/GameData/Source/{schema.FileName}";

            TsvTable table =
                TsvReader.Read(sourcePath);

            ValidationResult validation =
                TableValidator.Validate(schema, table);

            if (!validation.IsValid)
            {
                foreach (ValidationMessage message
                         in validation.Messages)
                {
                    if (message.Severity ==
                        GameDataValidationSeverity.Error)
                    {
                        errors.Add(message.ToString());
                    }
                }

                return false;
            }

            string idHeader = FindIdHeader(schema, table);

            foreach (TsvRow row in table.Rows)
            {
                ids.Add(row.GetValue(idHeader));
            }

            return true;
        }
        catch (Exception exception)
        {
            errors.Add(exception.Message);
            return false;
        }
    }

    private static string FindIdHeader(
        TableSchema schema,
        TsvTable table
    )
    {
        foreach (string header in table.Headers)
        {
            if (string.Equals(
                schema.ResolveFieldPath(header),
                schema.IdField,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return header;
            }
        }

        throw new InvalidOperationException(
            $"ID header for '{schema.IdField}' was not found."
        );
    }
}
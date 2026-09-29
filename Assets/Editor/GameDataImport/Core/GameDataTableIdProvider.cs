using System;
using System.Collections.Generic;

public static class GameDataTableIdProvider
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
            GameDataTableSchema schema =
                GameDataTableSchema.Create(definitionType);

            string sourcePath =
                $"Assets/GameData/Source/{schema.FileName}";

            GameDataTsvTable table =
                GameDataTsvReader.Read(sourcePath);

            GameDataValidationResult validation =
                GameDataTableValidator.Validate(schema, table);

            if (!validation.IsValid)
            {
                foreach (GameDataValidationMessage message
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

            foreach (GameDataTsvRow row in table.Rows)
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
        GameDataTableSchema schema,
        GameDataTsvTable table
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
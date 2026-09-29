using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GameDataScriptableObjectImporter
{
    public static GameDataImportResult Import(
        GameDataTableSchema schema,
        GameDataTsvTable table
    )
    {
        GameDataImportResult result =
            new GameDataImportResult();

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
                    result.AddError(message.ToString());
                }
            }

            return result;
        }

        string idHeader = FindIdHeader(schema, table);
        HashSet<string> sourceIds =
    new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    );

        foreach (GameDataTsvRow row in table.Rows)
        {
            sourceIds.Add(row.GetValue(idHeader));
        }

        Dictionary<string, ScriptableObject> existingAssets =
            GameDataAssetRepository.LoadExisting(
                schema,
                result
            );

        if (!result.IsSuccess ||
            !Preflight(schema, table, result))
        {
            return result;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        Undo.SetCurrentGroupName(
            $"Import {schema.DefinitionType.Name}"
        );

        foreach (GameDataTsvRow row in table.Rows)
        {
            string id = row.GetValue(idHeader);

            bool isNew = !existingAssets.TryGetValue(
                id,
                out ScriptableObject asset
            );

            if (isNew)
            {
                asset = ScriptableObject.CreateInstance(
                    schema.DefinitionType
                );
            }
            else
            {
                Undo.RecordObject(
                    asset,
                    $"Update {schema.DefinitionType.Name}"
                );
            }

            SerializedObject serializedAsset =
                new SerializedObject(asset);

            serializedAsset.Update();

            if (!TryWriteRow(
                serializedAsset,
                schema,
                table,
                row,
                result
            ))
            {
                if (isNew)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }

                Undo.RevertAllDownToGroup(undoGroup);
                return result;
            }

            serializedAsset.ApplyModifiedProperties();

            if (isNew)
            {
                GameDataAssetRepository.CreateAsset(
                    schema,
                    asset,
                    id
                );

                existingAssets.Add(id, asset);
                result.AddCreated(id);
            }
            else
            {
                result.AddUpdated(id);
            }

            EditorUtility.SetDirty(asset);
        }

        AddOrphanWarnings(
    schema,
    existingAssets,
    sourceIds,
    result
);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Undo.CollapseUndoOperations(undoGroup);

        return result;
    }

    private static bool Preflight(
        GameDataTableSchema schema,
        GameDataTsvTable table,
        GameDataImportResult result
    )
    {
        ScriptableObject temporaryAsset =
            ScriptableObject.CreateInstance(
                schema.DefinitionType
            );

        try
        {
            foreach (GameDataTsvRow row in table.Rows)
            {
                SerializedObject serializedAsset =
                    new SerializedObject(temporaryAsset);

                serializedAsset.Update();

                if (!TryWriteRow(
                    serializedAsset,
                    schema,
                    table,
                    row,
                    result
                ))
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                temporaryAsset
            );
        }
    }

    private static bool TryWriteRow(
        SerializedObject serializedAsset,
        GameDataTableSchema schema,
        GameDataTsvTable table,
        GameDataTsvRow row,
        GameDataImportResult result
    )
    {
        foreach (string header in table.Headers)
        {
            if (!GameDataSerializedPropertyWriter.TryWrite(
                serializedAsset,
                schema,
                header,
                row.GetValue(header),
                out string error
            ))
            {
                result.AddError(
                    $"Line {row.LineNumber}, " +
                    $"{header}: {error}"
                );

                return false;
            }
        }

        return true;
    }
    private static void AddOrphanWarnings(
        GameDataTableSchema schema,
        Dictionary<string, ScriptableObject> existingAssets,
        HashSet<string> sourceIds,
        GameDataImportResult result
    )
    {
        foreach (string existingId in existingAssets.Keys)
        {
            if (sourceIds.Contains(existingId))
            {
                continue;
            }

            result.AddWarning(
                $"{schema.DefinitionType.Name} '{existingId}' " +
                "exists as an asset but is missing from the TSV. " +
                "The asset was not deleted."
            );
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
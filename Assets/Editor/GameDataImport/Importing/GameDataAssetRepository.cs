using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameDataAssetRepository
{
    public static Dictionary<string, ScriptableObject> LoadExisting(
        GameDataTableSchema schema,
        GameDataImportResult result
    )
    {
        Dictionary<string, ScriptableObject> assets =
            new Dictionary<string, ScriptableObject>(
                StringComparer.OrdinalIgnoreCase
            );

        if (!AssetDatabase.IsValidFolder(schema.AssetFolder))
        {
            result.AddError(
                $"Asset folder does not exist: {schema.AssetFolder}"
            );

            return assets;
        }

        string[] guids = AssetDatabase.FindAssets(
            $"t:{schema.DefinitionType.Name}",
            new[] { schema.AssetFolder }
        );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            ScriptableObject asset =
                AssetDatabase.LoadAssetAtPath(
                    path,
                    schema.DefinitionType
                ) as ScriptableObject;

            if (asset == null)
            {
                continue;
            }

            SerializedObject serializedAsset =
                new SerializedObject(asset);

            SerializedProperty idProperty =
                serializedAsset.FindProperty(schema.IdField);

            if (idProperty == null ||
                idProperty.propertyType !=
                    SerializedPropertyType.String)
            {
                result.AddError(
                    $"{schema.DefinitionType.Name} ID field " +
                    $"'{schema.IdField}' must be a string."
                );

                continue;
            }

            string id = idProperty.stringValue.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            if (!assets.TryAdd(id, asset))
            {
                result.AddError(
                    $"Duplicate existing asset ID '{id}'."
                );
            }
        }

        return assets;
    }

    public static void CreateAsset(
        GameDataTableSchema schema,
        ScriptableObject asset,
        string id
    )
    {
        string fileName = MakeSafeFileName(id);

        string assetPath =
            AssetDatabase.GenerateUniqueAssetPath(
                $"{schema.AssetFolder}/{fileName}.asset"
            );

        AssetDatabase.CreateAsset(asset, assetPath);

        Undo.RegisterCreatedObjectUndo(
            asset,
            $"Create {schema.DefinitionType.Name}"
        );
    }

    private static string MakeSafeFileName(string value)
    {
        char[] characters = value.ToCharArray();
        char[] invalidCharacters =
            Path.GetInvalidFileNameChars();

        for (int index = 0;
             index < characters.Length;
             index++)
        {
            if (Array.IndexOf(
                invalidCharacters,
                characters[index]
            ) >= 0)
            {
                characters[index] = '_';
            }
        }

        return new string(characters);
    }
}
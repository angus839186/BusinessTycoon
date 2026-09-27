using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ResourceDefinitionImporter
{
    private const string ResourceAssetFolder =
        "Assets/Resources/Resource";

    public static bool Import(
        IReadOnlyList<ResourceImportRow> rows,
        out List<string> messages,
        out List<string> errors
    )
    {
        messages = new List<string>();
        errors = new List<string>();

        if (!AssetDatabase.IsValidFolder(ResourceAssetFolder))
        {
            errors.Add($"找不到資料夾：{ResourceAssetFolder}");
            return false;
        }

        Dictionary<string, ResourceDefinition> definitions =
            LoadExistingDefinitions(errors);

        Dictionary<ResourceImportRow, Sprite> icons =
            ValidateIcons(rows, errors);

        // 先完成所有檢查，避免只匯入一半。
        if (errors.Count > 0)
            return false;

        foreach (ResourceImportRow row in rows)
        {
            bool isNew = !definitions.TryGetValue(
                row.ResourceId,
                out ResourceDefinition definition
            );

            if (isNew)
            {
                definition =
                    ScriptableObject.CreateInstance<ResourceDefinition>();
            }
            else
            {
                Undo.RecordObject(
                    definition,
                    "Import Resource Definition"
                );
            }

            definition.resourceId = row.ResourceId;
            definition.displayName = row.DisplayName;
            definition.basePrice = row.BasePrice;
            definition.isEnabled = row.Enabled;

            // 空白路徑代表保留現有 Icon。
            if (icons.TryGetValue(row, out Sprite icon))
                definition.icon = icon;

            if (isNew)
            {
                string fileName = MakeSafeFileName(row.ResourceId);

                string assetPath =
                    AssetDatabase.GenerateUniqueAssetPath(
                        $"{ResourceAssetFolder}/{fileName}.asset"
                    );

                AssetDatabase.CreateAsset(definition, assetPath);

                Undo.RegisterCreatedObjectUndo(
                    definition,
                    "Create Resource Definition"
                );

                definitions.Add(row.ResourceId, definition);
                messages.Add($"新增：{row.ResourceId}");
            }
            else
            {
                messages.Add($"更新：{row.ResourceId}");
            }

            EditorUtility.SetDirty(definition);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return true;
    }

    private static Dictionary<string, ResourceDefinition>
        LoadExistingDefinitions(List<string> errors)
    {
        Dictionary<string, ResourceDefinition> definitions =
            new Dictionary<string, ResourceDefinition>(
                StringComparer.Ordinal
            );

        string[] guids = AssetDatabase.FindAssets(
            "t:ResourceDefinition",
            new[] { ResourceAssetFolder }
        );

        foreach (string guid in guids)
        {
            string assetPath =
                AssetDatabase.GUIDToAssetPath(guid);

            ResourceDefinition definition =
                AssetDatabase.LoadAssetAtPath<ResourceDefinition>(
                    assetPath
                );

            if (definition == null ||
                string.IsNullOrWhiteSpace(definition.resourceId))
            {
                continue;
            }

            string resourceId = definition.resourceId.Trim();

            if (!definitions.TryAdd(resourceId, definition))
            {
                errors.Add(
                    $"現有 SO 的 resourceId 重複：{resourceId}"
                );
            }
        }

        return definitions;
    }

    private static Dictionary<ResourceImportRow, Sprite> ValidateIcons(
        IReadOnlyList<ResourceImportRow> rows,
        List<string> errors
    )
    {
        Dictionary<ResourceImportRow, Sprite> icons =
            new Dictionary<ResourceImportRow, Sprite>();

        foreach (ResourceImportRow row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.IconPath))
                continue;

            if (!row.IconPath.StartsWith(
                "Assets/",
                StringComparison.Ordinal
            ))
            {
                errors.Add(
                    $"第 {row.LineNumber} 行：" +
                    "iconPath 必須從 Assets/ 開始。"
                );

                continue;
            }

            Sprite icon =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    row.IconPath
                );

            if (icon == null)
            {
                errors.Add(
                    $"第 {row.LineNumber} 行找不到 Sprite：" +
                    row.IconPath
                );

                continue;
            }

            icons.Add(row, icon);
        }

        return icons;
    }

    private static string MakeSafeFileName(string value)
    {
        char[] characters = value.ToCharArray();
        char[] invalidCharacters = Path.GetInvalidFileNameChars();

        for (int index = 0; index < characters.Length; index++)
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
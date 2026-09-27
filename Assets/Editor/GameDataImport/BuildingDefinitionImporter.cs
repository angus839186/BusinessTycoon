using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildingDefinitionImporter
{
    private const string BuildingFolder =
        "Assets/Resources/Buildings";

    public static bool Import(
        IReadOnlyList<BuildingImportRow> rows,
        out List<string> messages,
        out List<string> errors
    )
    {
        messages = new List<string>();
        errors = new List<string>();

        if (!AssetDatabase.IsValidFolder(BuildingFolder))
        {
            errors.Add($"找不到資料夾：{BuildingFolder}");
            return false;
        }

        Dictionary<string, ResourceDefinition> resources =
            GameDataImportAssetLookup.LoadResources(errors);

        Dictionary<string, BuildingDefinition> buildings =
            GameDataImportAssetLookup.LoadBuildings(errors);

        Dictionary<BuildingImportRow, Sprite> icons =
            ValidateReferences(rows, resources, errors);

        // 全部檢查通過後才開始修改資產。
        if (errors.Count > 0)
            return false;

        foreach (BuildingImportRow row in rows)
        {
            bool isNew = !buildings.TryGetValue(
                row.BuildingId,
                out BuildingDefinition definition
            );

            if (isNew)
            {
                definition =
                    ScriptableObject.CreateInstance<BuildingDefinition>();
            }
            else
            {
                Undo.RecordObject(
                    definition,
                    "Import Building Definition"
                );
            }

            definition.buildingId = row.BuildingId;
            definition.displayName = row.DisplayName;
            definition.category = row.Category;
            definition.isEnabled = row.Enabled;
            definition.markerColor = row.MarkerColor;
            definition.processInterval = row.ProcessInterval;
            definition.workSpeedMultiplier = row.WorkSpeedMultiplier;
            definition.acceptsAnyResource = row.AcceptsAnyResource;

            definition.producedResource =
                string.IsNullOrWhiteSpace(row.ProducedResourceId)
                    ? null
                    : resources[row.ProducedResourceId];

            // 空白 iconPath 代表保留現有 Icon。
            if (icons.TryGetValue(row, out Sprite icon))
                definition.icon = icon;

            if (isNew)
            {
                string fileName = MakeSafeFileName(row.BuildingId);

                string assetPath =
                    AssetDatabase.GenerateUniqueAssetPath(
                        $"{BuildingFolder}/{fileName}.asset"
                    );

                AssetDatabase.CreateAsset(definition, assetPath);

                Undo.RegisterCreatedObjectUndo(
                    definition,
                    "Create Building Definition"
                );

                buildings.Add(row.BuildingId, definition);
                messages.Add($"新增建築：{row.BuildingId}");
            }
            else
            {
                messages.Add($"更新建築：{row.BuildingId}");
            }

            EditorUtility.SetDirty(definition);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return true;
    }

    private static Dictionary<BuildingImportRow, Sprite>
        ValidateReferences(
            IReadOnlyList<BuildingImportRow> rows,
            Dictionary<string, ResourceDefinition> resources,
            List<string> errors
        )
    {
        Dictionary<BuildingImportRow, Sprite> icons =
            new Dictionary<BuildingImportRow, Sprite>();

        foreach (BuildingImportRow row in rows)
        {
            if (!string.IsNullOrWhiteSpace(row.ProducedResourceId) &&
                !resources.ContainsKey(row.ProducedResourceId))
            {
                errors.Add(
                    $"第 {row.LineNumber} 行找不到 Resource SO：" +
                    row.ProducedResourceId
                );
            }

            if (string.IsNullOrWhiteSpace(row.IconPath))
                continue;

            if (!row.IconPath.StartsWith(
                "Assets/",
                StringComparison.Ordinal
            ))
            {
                errors.Add(
                    $"第 {row.LineNumber} 行 iconPath 必須從 Assets/ 開始。"
                );

                continue;
            }

            Sprite icon =
                AssetDatabase.LoadAssetAtPath<Sprite>(row.IconPath);

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
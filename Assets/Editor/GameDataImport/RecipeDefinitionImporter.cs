using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class RecipeDefinitionImporter
{
    private const string ResourceAssetFolder =
        "Assets/Resources/Resource";

    public static bool Import(
        IReadOnlyList<RecipeImportRow> rows,
        out List<string> messages,
        out List<string> errors
    )
    {
        messages = new List<string>();
        errors = new List<string>();

        Dictionary<string, ResourceDefinition> definitions =
            LoadDefinitions(errors);

        foreach (RecipeImportRow row in rows)
        {
            if (!definitions.ContainsKey(row.OwnerResourceId))
            {
                errors.Add(
                    $"第 {row.LineNumber} 行找不到配方擁有者：" +
                    row.OwnerResourceId
                );
            }

            if (!definitions.ContainsKey(row.ResourceId))
            {
                errors.Add(
                    $"第 {row.LineNumber} 行找不到資源：" +
                    row.ResourceId
                );
            }
        }

        IEnumerable<IGrouping<string, RecipeImportRow>> groups =
            rows.GroupBy(
                row => row.OwnerResourceId,
                StringComparer.Ordinal
            );

        foreach (IGrouping<string, RecipeImportRow> group in groups)
        {
            RecipeImportRow[] enabledRows =
                group.Where(row => row.Enabled).ToArray();

            if (enabledRows.Length > 0 &&
                !enabledRows.Any(
                    row => row.IoType == RecipeIoType.Output
                ))
            {
                errors.Add(
                    $"配方 {group.Key} 至少需要一筆 Output。"
                );
            }
        }

        if (errors.Count > 0)
            return false;

        foreach (IGrouping<string, RecipeImportRow> group in groups)
        {
            ResourceDefinition owner = definitions[group.Key];

            Undo.RecordObject(owner, "Import Resource Recipe");

            if (owner.recipe == null)
                owner.recipe = new ResourceRecipe();

            owner.recipe.inputResources = BuildAmounts(
                group,
                RecipeIoType.Input,
                definitions
            );

            owner.recipe.outputResources = BuildAmounts(
                group,
                RecipeIoType.Output,
                definitions
            );

            EditorUtility.SetDirty(owner);

            messages.Add(
                $"更新配方：{group.Key}，" +
                $"Input {owner.recipe.inputResources.Length}，" +
                $"Output {owner.recipe.outputResources.Length}"
            );
        }

        AssetDatabase.SaveAssets();
        return true;
    }

    private static ResourceAmount[] BuildAmounts(
        IEnumerable<RecipeImportRow> rows,
        RecipeIoType ioType,
        Dictionary<string, ResourceDefinition> definitions
    )
    {
        return rows
            .Where(row => row.Enabled && row.IoType == ioType)
            .Select(row => new ResourceAmount
            {
                resource = definitions[row.ResourceId],
                amount = row.Amount
            })
            .ToArray();
    }

    private static Dictionary<string, ResourceDefinition>
        LoadDefinitions(List<string> errors)
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
            string path = AssetDatabase.GUIDToAssetPath(guid);

            ResourceDefinition definition =
                AssetDatabase.LoadAssetAtPath<ResourceDefinition>(path);

            if (definition == null ||
                string.IsNullOrWhiteSpace(definition.resourceId))
            {
                continue;
            }

            string resourceId = definition.resourceId.Trim();

            if (!definitions.TryAdd(resourceId, definition))
                errors.Add($"現有 Resource ID 重複：{resourceId}");
        }

        return definitions;
    }
}
using System;
using System.Collections.Generic;
using UnityEditor;

public static class GameDataImportAssetLookup
{
    private const string ResourceFolder =
        "Assets/Resources/Resource";

    private const string BuildingFolder =
        "Assets/Resources/Buildings";

    public static Dictionary<string, ResourceDefinition> LoadResources(
        List<string> errors
    )
    {
        Dictionary<string, ResourceDefinition> result =
            new Dictionary<string, ResourceDefinition>(
                StringComparer.Ordinal
            );

        string[] guids = AssetDatabase.FindAssets(
            "t:ResourceDefinition",
            new[] { ResourceFolder }
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

            string id = definition.resourceId.Trim();

            if (!result.TryAdd(id, definition))
                errors.Add($"現有 Resource ID 重複：{id}");
        }

        return result;
    }

    public static Dictionary<string, BuildingDefinition> LoadBuildings(
        List<string> errors
    )
    {
        Dictionary<string, BuildingDefinition> result =
            new Dictionary<string, BuildingDefinition>(
                StringComparer.Ordinal
            );

        string[] guids = AssetDatabase.FindAssets(
            "t:BuildingDefinition",
            new[] { BuildingFolder }
        );

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            BuildingDefinition definition =
                AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);

            if (definition == null ||
                string.IsNullOrWhiteSpace(definition.buildingId))
            {
                continue;
            }

            string id = definition.buildingId.Trim();

            if (!result.TryAdd(id, definition))
                errors.Add($"現有 Building ID 重複：{id}");
        }

        return result;
    }
}
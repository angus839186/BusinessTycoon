using System.IO;
using UnityEngine;
using System;

public static class MapLayerLoader
{
    private const string ConfigPath = "Config/map_config.json";
    private const string DefaultMapFolder = "Maps/Taiwan";

    private static MapConfig config;

    public static MapConfig Config => config ??= LoadConfig();
    public static string MapFolder => Config.mapFolder;

    private static MapConfig LoadConfig()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, ConfigPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Map config missing: {fullPath}");
            return CreateFallbackConfig();
        }

        try
        {
            MapConfig loadedConfig = JsonUtility.FromJson<MapConfig>(File.ReadAllText(fullPath));

            if (loadedConfig == null)
            {
                Debug.LogError($"Map config is invalid: {fullPath}");
                return CreateFallbackConfig();
            }

            if (string.IsNullOrWhiteSpace(loadedConfig.mapFolder))
            {
                Debug.LogWarning($"Map folder is empty. Using fallback: {DefaultMapFolder}");
                loadedConfig.mapFolder = DefaultMapFolder;
            }

            Debug.Log($"Map config loaded: Id={loadedConfig.mapId}, Folder={loadedConfig.mapFolder}");
            return loadedConfig;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return CreateFallbackConfig();
        }
    }

    private static MapConfig CreateFallbackConfig()
    {
        return new MapConfig
        {
            mapId = "Taiwan",
            mapFolder = DefaultMapFolder
        };
    }
}

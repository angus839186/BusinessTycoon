using System.IO;
using UnityEngine;

public static class MapLayerLoader
{
    public static void ValidateConfiguredLayers(string mapRootPath, MapConfig config)
    {
        if (config == null || config.layers == null || config.layers.Length == 0)
        {
            Debug.Log("Map layers: no configured GeoJSON layers.");
            return;
        }

        foreach (MapLayerConfig layer in config.layers)
        {
            if (layer == null || string.IsNullOrWhiteSpace(layer.path))
            {
                Debug.LogWarning("Map layer skipped: empty layer path.");
                continue;
            }

            string layerPath = Path.Combine(mapRootPath, layer.path);

            if (!File.Exists(layerPath))
            {
                Debug.LogWarning($"Map layer missing: Id={layer.id}, Path={layerPath}");
                continue;
            }

            FileInfo fileInfo = new FileInfo(layerPath);

            Debug.Log(
                $"Map layer ready: Id={layer.id}, Category={layer.category}, Geometry={layer.geometry}, Bytes={fileInfo.Length}"
            );
        }
    }
}

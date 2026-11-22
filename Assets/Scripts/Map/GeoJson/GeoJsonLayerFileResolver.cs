using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum GeoJsonLayerType
{
    Land,
    Water
}

public static class GeoJsonLayerFileResolver
{
    public static bool TryFind(string mapFolder, GeoJsonLayerType layerType, out string fullPath)
    {
        fullPath = null;

        if (string.IsNullOrWhiteSpace(mapFolder))
        {
            Debug.LogError("GeoJSON map folder is empty.");
            return false;
        }

        string folderPath = Path.Combine(Application.streamingAssetsPath, mapFolder);

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"GeoJSON map folder not found: {folderPath}");
            return false;
        }

        string prefix = $"{layerType}_";
        List<string> matches = new List<string>();

        foreach (string path in Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly))
        {
            string extension = Path.GetExtension(path);

            if (!extension.Equals(".geojson", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                fileName.Length > prefix.Length)
            {
                matches.Add(path);
            }
        }

        if (matches.Count == 0)
        {
            Debug.LogError($"GeoJSON {layerType} file not found in {folderPath}. Expected naming: {layerType}_XXX.geojson");
            return false;
        }

        if (matches.Count > 1)
        {
            Debug.LogError($"Multiple GeoJSON {layerType} files found in {folderPath}: {string.Join(", ", matches)}");
            return false;
        }

        fullPath = matches[0];
        return true;
    }
}
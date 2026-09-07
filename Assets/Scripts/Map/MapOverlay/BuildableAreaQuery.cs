using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class BuildableAreaQuery : MonoBehaviour
{
    [SerializeField] private string mapFolder = "Maps/TaiwanTest";
    [SerializeField] private string landAreaPath = "Layers/base/taiwan_admin_polygon.geojson";

    private readonly List<GeoJsonPolygonFeature> landAreas = new List<GeoJsonPolygonFeature>();

    private void Start()
    {
        LoadLandAreas();
    }

    public bool Contains(double longitude, double latitude)
    {
        foreach (GeoJsonPolygonFeature area in landAreas)
        {
            if (area.Contains(longitude, latitude))
                return true;
        }

        return false;
    }

    private void LoadLandAreas()
    {
        landAreas.Clear();

        string fullPath = Path.Combine(
            Application.streamingAssetsPath,
            mapFolder,
            landAreaPath
        );

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Buildable area GeoJSON missing: {fullPath}");
            return;
        }

        string json = File.ReadAllText(fullPath);
        JObject root = JObject.Parse(json);
        JArray features = root["features"] as JArray;

        if (features == null)
        {
            Debug.LogError($"Buildable area GeoJSON has no features: {fullPath}");
            return;
        }

        foreach (JToken feature in features)
        {
            JToken properties = feature["properties"];
            int fid = properties?["fid"]?.Value<int>() ?? 0;
            string featureClass = properties?["fclass"]?.Value<string>() ?? "";
            string displayName = properties?["name"]?.Value<string>() ?? "";

            if (GeoJsonPolygonFeatureReader.TryRead(
                feature,
                fid,
                featureClass,
                displayName,
                out GeoJsonPolygonFeature polygonFeature
            ))
            {
                landAreas.Add(polygonFeature);
            }
        }

        Debug.Log($"Buildable areas loaded: {landAreas.Count}");
    }
}
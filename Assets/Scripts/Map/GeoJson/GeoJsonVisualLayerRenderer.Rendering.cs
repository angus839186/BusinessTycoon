using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed partial class GeoJsonVisualLayerRenderer
{
    private void LoadLayer()
    {
        if (!GeoJsonLayerFileResolver.TryFind(MapLayerLoader.MapFolder, layerType, out string fullPath))
            return;

        string json = File.ReadAllText(fullPath);
        JObject root = JObject.Parse(json);
        JArray features = root["features"] as JArray;

        if (features == null)
        {
            Debug.LogError($"GeoJSON has no features: {fullPath}");
            return;
        }

        int polygonCount = 0;
        int lineCount = 0;

        foreach (JToken feature in features)
        {
            string geometryType = feature["geometry"]?["type"]?.Value<string>();
            JToken coordinates = feature["geometry"]?["coordinates"];

            if (drawPolygons && geometryType == "Polygon")
            {
                if (polygonCount >= maxPolygons)
                    continue;

                if (DrawPolygon(coordinates, polygonCount))
                    polygonCount++;
            }
            else if (drawPolygons && geometryType == "MultiPolygon")
            {
                foreach (JToken polygonToken in coordinates)
                {
                    if (polygonCount >= maxPolygons)
                        break;

                    if (DrawPolygon(polygonToken, polygonCount))
                        polygonCount++;
                }
            }
            else if (drawLines && geometryType == "LineString")
            {
                if (lineCount >= maxLines)
                    continue;

                if (GeoJsonLineRendererBuilder.DrawLine(
    transform,
    coordinates,
    lineCount,
    projection,
    pointStep,
    zPosition,
    lineWidth,
    fillMaterial,
    fillColor,
    sortingOrder
))
                    lineCount++;
            }
            else if (drawLines && geometryType == "MultiLineString")
            {
                foreach (JToken lineToken in coordinates)
                {
                    if (lineCount >= maxLines)
                        break;

                    if (GeoJsonLineRendererBuilder.DrawLine(
    transform,
    lineToken,
    lineCount,
    projection,
    pointStep,
    zPosition,
    lineWidth,
    fillMaterial,
    fillColor,
    sortingOrder
))
                        lineCount++;
                }
            }
        }

        Debug.Log($"GeoJSON visual layer loaded: {Path.GetFileName(fullPath)}, Polygons={polygonCount}, Lines={lineCount}");
    }

    private bool DrawPolygon(JToken polygonToken, int index)
    {
        JArray rings = polygonToken as JArray;

        if (rings == null || rings.Count == 0)
            return false;

        JArray outerRing = rings[0] as JArray;

        if (outerRing == null || outerRing.Count < 3)
            return false;

        List<Vector3> vertices = new List<Vector3>();

        int step = Mathf.Max(1, pointStep);

        for (int i = 0; i < outerRing.Count; i += step)
        {
            JToken pointToken = outerRing[i];

            JArray point = pointToken as JArray;

            if (point == null || point.Count < 2)
                continue;

            double longitude = point[0].Value<double>();
            double latitude = point[1].Value<double>();

            if (projection.TryLonLatToWorldPosition(longitude, latitude, out Vector3 worldPosition))
            {
                worldPosition.z = zPosition;
                vertices.Add(worldPosition);
            }
        }

        if (vertices.Count < 3)
            return false;

        Mesh mesh = GeoJsonPolygonMeshBuilder.CreateFanMesh(
    vertices,
    $"GeoJsonVisualMesh_{index}"
);
        mesh.name = $"GeoJsonVisualMesh_{index}";

        GameObject polygonObject = new GameObject($"GeoJsonVisualPolygon_{index}");
        polygonObject.transform.SetParent(transform);

        MeshFilter meshFilter = polygonObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = polygonObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = fillMaterial;
        meshRenderer.sortingOrder = sortingOrder;

        return true;
    }
}
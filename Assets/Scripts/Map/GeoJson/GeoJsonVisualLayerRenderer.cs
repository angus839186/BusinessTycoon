using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class GeoJsonVisualLayerRenderer : MonoBehaviour
{
    [SerializeField] private GeoJsonMapProjection projection;
    [SerializeField] private string mapFolder = "Maps/TaiwanTest";
    [SerializeField] private string layerPath = "Layers/taiwan_admin_polygon.geojson";

    [SerializeField] private Color fillColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private int sortingOrder = 10;
    [SerializeField] private float zPosition = 0f;
    [SerializeField] private int maxPolygons = 500;
    [SerializeField] private int pointStep = 4;

    [SerializeField] private bool drawPolygons = true;
    [SerializeField] private bool drawLines = false;
    [SerializeField] private int maxLines = 500;
    [SerializeField] private float lineWidth = 0.02f;

    private Material fillMaterial;

    private IEnumerator Start()
    {
        while (projection == null)
            yield return null;

        fillMaterial = GeoJsonMaterialFactory.Create(fillColor);
        LoadLayer();
    }

    private void LoadLayer()
    {
        string fullPath = Path.Combine(
            Application.streamingAssetsPath,
            mapFolder,
            layerPath
        );

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"找不到 GeoJSON：{fullPath}");
            return;
        }

        string json = File.ReadAllText(fullPath);
        JObject root = JObject.Parse(json);
        JArray features = root["features"] as JArray;

        if (features == null)
        {
            Debug.LogError($"GeoJSON 沒有 features：{fullPath}");
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

        Debug.Log(
    $"GeoJSON visual layer loaded: {layerPath}, " +
    $"Polygons={polygonCount}, Lines={lineCount}"
);
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
    public void LoadLayer(string newLayerPath)
    {
        if (string.IsNullOrWhiteSpace(newLayerPath))
            return;

        layerPath = newLayerPath;
        ClearLayer();

        if (fillMaterial == null)
            fillMaterial = GeoJsonMaterialFactory.Create(fillColor);

        LoadLayer();
    }

    private void ClearLayer()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
    public void SetFillColor(Color color)
    {
        fillColor = color;

        if (fillMaterial != null)
            fillMaterial.color = fillColor;
    }
}
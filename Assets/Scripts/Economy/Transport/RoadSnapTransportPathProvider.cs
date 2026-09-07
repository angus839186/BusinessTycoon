using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class RoadSnapTransportPathProvider : MonoBehaviour, ITransportPathProvider
{
    [SerializeField] private GeoJsonMapProjection projection;
    [SerializeField] private string mapFolder = "Maps/TaiwanTest";
    [SerializeField] private string roadLayerPath = "Layers/base/taiwan_roads_line.geojson";
    [SerializeField] private int maxLines = 300;
    [SerializeField] private int pointStep = 12;

    private readonly List<Vector3> roadPoints = new List<Vector3>();
    private bool loaded;

    private IEnumerator Start()
    {
        while (projection == null)
            yield return null;

        LoadRoadPoints();
    }

    public IReadOnlyList<Vector3> GetPath(
        BuildingInstance source,
        BuildingInstance target
    )
    {
        if (source == null || target == null)
            return null;

        Vector3 sourcePosition = source.transform.position;
        Vector3 targetPosition = target.transform.position;

        if (!loaded || roadPoints.Count == 0)
        {
            return new List<Vector3>
            {
                sourcePosition,
                targetPosition
            };
        }

        Vector3 sourceRoad = FindNearestRoadPoint(sourcePosition);
        Vector3 targetRoad = FindNearestRoadPoint(targetPosition);

        return new List<Vector3>
        {
            sourcePosition,
            sourceRoad,
            targetRoad,
            targetPosition
        };
    }

    private void LoadRoadPoints()
    {
        string fullPath = Path.Combine(
            Application.streamingAssetsPath,
            mapFolder,
            roadLayerPath
        );

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Road GeoJSON missing: {fullPath}");
            return;
        }

        string json = File.ReadAllText(fullPath);
        JObject root = JObject.Parse(json);
        JArray features = root["features"] as JArray;

        if (features == null)
        {
            Debug.LogError($"Road GeoJSON has no features: {fullPath}");
            return;
        }

        int lineCount = 0;

        foreach (JToken feature in features)
        {
            if (lineCount >= maxLines)
                break;

            string geometryType = feature["geometry"]?["type"]?.Value<string>();
            JToken coordinates = feature["geometry"]?["coordinates"];

            if (geometryType == "LineString")
            {
                ReadLine(coordinates);
                lineCount++;
            }
            else if (geometryType == "MultiLineString")
            {
                foreach (JToken lineToken in coordinates)
                {
                    if (lineCount >= maxLines)
                        break;

                    ReadLine(lineToken);
                    lineCount++;
                }
            }
        }

        loaded = true;
        Debug.Log($"Road snap points loaded: Lines={lineCount}, Points={roadPoints.Count}");
    }

    private void ReadLine(JToken lineToken)
    {
        JArray points = lineToken as JArray;

        if (points == null)
            return;

        int step = Mathf.Max(1, pointStep);

        for (int i = 0; i < points.Count; i += step)
        {
            JArray point = points[i] as JArray;

            if (point == null || point.Count < 2)
                continue;

            double longitude = point[0].Value<double>();
            double latitude = point[1].Value<double>();

            if (projection.TryLonLatToWorldPosition(longitude, latitude, out Vector3 worldPosition))
                roadPoints.Add(worldPosition);
        }
    }

    private Vector3 FindNearestRoadPoint(Vector3 position)
    {
        Vector3 nearest = roadPoints[0];
        float nearestSqrDistance = (nearest - position).sqrMagnitude;

        for (int i = 1; i < roadPoints.Count; i++)
        {
            float sqrDistance = (roadPoints[i] - position).sqrMagnitude;

            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearest = roadPoints[i];
            nearestSqrDistance = sqrDistance;
        }

        return nearest;
    }
}
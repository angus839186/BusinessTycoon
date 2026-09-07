using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class GeoJsonLineRendererBuilder
{
    public static bool DrawLine(
        Transform parent,
        JToken lineToken,
        int index,
        GeoJsonMapProjection projection,
        int pointStep,
        float zPosition,
        float lineWidth,
        Material material,
        Color color,
        int sortingOrder
    )
    {
        JArray points = lineToken as JArray;

        if (points == null || points.Count < 2)
            return false;

        List<Vector3> positions = new List<Vector3>();
        int step = Mathf.Max(1, pointStep);

        for (int i = 0; i < points.Count; i += step)
        {
            JArray point = points[i] as JArray;

            if (point == null || point.Count < 2)
                continue;

            double longitude = point[0].Value<double>();
            double latitude = point[1].Value<double>();

            if (projection.TryLonLatToWorldPosition(longitude, latitude, out Vector3 worldPosition))
            {
                worldPosition.z = zPosition;
                positions.Add(worldPosition);
            }
        }

        if (positions.Count < 2)
            return false;

        GameObject lineObject = new GameObject($"GeoJsonVisualLine_{index}");
        lineObject.transform.SetParent(parent);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = positions.Count;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = material;
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = sortingOrder;
        line.SetPositions(positions.ToArray());

        return true;
    }
}
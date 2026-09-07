using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class GeoJsonPolygonFeatureReader
{
    public static bool TryRead(
        JToken feature,
        int fid,
        string featureClass,
        string displayName,
        out GeoJsonPolygonFeature polygonFeature
    )
    {
        polygonFeature = null;

        string geometryType = feature["geometry"]?["type"]?.Value<string>();
        JArray coordinates = feature["geometry"]?["coordinates"] as JArray;

        if (coordinates == null)
            return false;

        List<List<Vector2>> allRings = new List<List<Vector2>>();

        if (geometryType == "Polygon")
            ReadPolygon(coordinates, allRings);
        else if (geometryType == "MultiPolygon")
        {
            foreach (JToken polygonToken in coordinates)
            {
                JArray polygon = polygonToken as JArray;

                if (polygon == null)
                    continue;

                ReadPolygon(polygon, allRings);
            }
        }

        if (allRings.Count == 0)
            return false;

        polygonFeature = new GeoJsonPolygonFeature(
            fid,
            featureClass,
            displayName,
            allRings
        );

        return true;
    }

    private static void ReadPolygon(
        JArray polygon,
        List<List<Vector2>> allRings
    )
    {
        foreach (JToken ringToken in polygon)
        {
            JArray ring = ringToken as JArray;

            if (ring == null)
                continue;

            List<Vector2> points = new List<Vector2>();

            foreach (JToken pointToken in ring)
            {
                JArray coordinate = pointToken as JArray;

                if (coordinate == null || coordinate.Count < 2)
                    continue;

                points.Add(new Vector2(
                    coordinate[0].Value<float>(),
                    coordinate[1].Value<float>()
                ));
            }

            if (points.Count >= 3)
                allRings.Add(points);
        }
    }
}
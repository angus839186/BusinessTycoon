using System.Collections.Generic;
using UnityEngine;

public sealed class GeoJsonPolygonFeature
{
    public int Fid { get; }
    public string FeatureClass { get; }
    public string DisplayName { get; }

    private readonly List<List<Vector2>> rings;
    public IReadOnlyList<List<Vector2>> Rings => rings;
    private readonly Rect bounds;

    public GeoJsonPolygonFeature(int fid, string featureClass, string displayName, List<List<Vector2>> rings)
    {
        Fid = fid;
        FeatureClass = featureClass;
        DisplayName = displayName;
        this.rings = rings;
        bounds = CalculateBounds(rings);
    }

    public bool Contains(double longitude, double latitude)
    {
        Vector2 point = new Vector2((float)longitude, (float)latitude);

        if (!bounds.Contains(point))
            return false;

        if (rings.Count == 0 || !PointInRing(point, rings[0]))
            return false;

        for (int i = 1; i < rings.Count; i++)
        {
            if (PointInRing(point, rings[i]))
                return false;
        }

        return true;
    }

    private static bool PointInRing(Vector2 point, List<Vector2> ring)
    {
        bool inside = false;

        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
        {
            Vector2 a = ring[i];
            Vector2 b = ring[j];

            bool intersects =
                ((a.y > point.y) != (b.y > point.y)) &&
                (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x);

            if (intersects)
                inside = !inside;
        }

        return inside;
    }

    private static Rect CalculateBounds(List<List<Vector2>> rings)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (List<Vector2> ring in rings)
        {
            foreach (Vector2 point in ring)
            {
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
            }
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
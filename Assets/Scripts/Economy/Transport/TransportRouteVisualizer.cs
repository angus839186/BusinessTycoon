using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public sealed class TransportRouteVisualizer : MonoBehaviour
{
    [SerializeField] private Color lineColor = new Color(1f, 0.85f, 0.1f, 0.9f);
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private float visibleSeconds = 1.5f;
    [SerializeField] private int sortingOrder = 350;

    [SerializeField]
    private Color routeColor =
    new Color(1f, 0.85f, 0.1f, 0.55f);

    private readonly Dictionary<string, GameObject> routeLines =
    new Dictionary<string, GameObject>();

    public void ShowRoute(
        string routeId,
        IReadOnlyList<Vector3> points
    )
    {
        if (
            string.IsNullOrWhiteSpace(routeId) ||
            points == null ||
            points.Count < 2
        )
        {
            return;
        }

        if (routeLines.TryGetValue(routeId, out GameObject oldLine))
        {
            if (oldLine != null)
                Destroy(oldLine);

            routeLines.Remove(routeId);
        }

        GameObject lineObject = CreateLine(
            $"Route_{routeId}",
            points,
            routeColor
        );

        routeLines.Add(routeId, lineObject);
    }

    public void ClearRoutes()
    {
        foreach (GameObject lineObject in routeLines.Values)
        {
            if (lineObject != null)
                Destroy(lineObject);
        }

        routeLines.Clear();
    }
    private IEnumerator DestroyAfter(GameObject target, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (target != null)
            Destroy(target);
    }
    public void ShowTransferPath(IReadOnlyList<Vector3> points)
    {
        if (points == null || points.Count < 2)
            return;

        GameObject lineObject = CreateLine(
            "TransportTransferPath",
            points,
            lineColor
        );

        StartCoroutine(
            DestroyAfter(lineObject, visibleSeconds)
        );
    }
    private GameObject CreateLine(
    string objectName,
    IReadOnlyList<Vector3> points,
    Color color
)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform);

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = points.Count;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material =
            new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = sortingOrder;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            point.z = -0.7f;
            line.SetPosition(i, point);
        }

        return lineObject;
    }
}
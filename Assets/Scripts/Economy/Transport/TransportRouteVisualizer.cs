using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public sealed class TransportRouteVisualizer : MonoBehaviour
{
    [SerializeField] private Color lineColor = new Color(1f, 0.85f, 0.1f, 0.9f);
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private float visibleSeconds = 1.5f;
    [SerializeField] private int sortingOrder = 350;

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

        GameObject lineObject = new GameObject("TransportRoutePath");
        lineObject.transform.SetParent(transform);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = points.Count;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.sortingOrder = sortingOrder;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            point.z = -0.7f;
            line.SetPosition(i, point);
        }

        StartCoroutine(DestroyAfter(lineObject, visibleSeconds));
    }
}
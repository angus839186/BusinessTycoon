using System.Collections;
using UnityEngine;

public sealed partial class GeoJsonVisualLayerRenderer : MonoBehaviour
{
    [SerializeField] private GeoJsonMapProjection projection;
    [SerializeField] private GeoJsonLayerType layerType = GeoJsonLayerType.Land;

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
    public void LoadLayer(GeoJsonLayerType newLayerType)
    {
        layerType = newLayerType;
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
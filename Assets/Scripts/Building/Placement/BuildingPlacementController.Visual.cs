using UnityEngine;
using System.Collections.Generic;

public sealed partial class BuildingPlacementController : MonoBehaviour
{
    private void CreateProgressBar(Transform buildingTransform, BuildingInstance instance)
    {
        GameObject root = new GameObject("ProgressBar");
        root.transform.SetParent(buildingTransform, false);
        root.transform.localPosition = new Vector3(0f, 0.75f, -0.05f);
        root.transform.localScale = new Vector3(1.6f, 0.25f, 1f);

        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "Background";
        background.transform.SetParent(root.transform, false);

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "Fill";
        fill.transform.SetParent(root.transform, false);

        Destroy(background.GetComponent<Collider>());
        Destroy(fill.GetComponent<Collider>());

        Renderer backgroundRenderer = background.GetComponent<Renderer>();
        backgroundRenderer.material = new Material(Shader.Find("Sprites/Default"));
        backgroundRenderer.material.color = new Color(0f, 0f, 0f, 0.6f);
        backgroundRenderer.sortingOrder = 300;

        Renderer fillRenderer = fill.GetComponent<Renderer>();
        fillRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fillRenderer.material.color = Color.green;
        fillRenderer.sortingOrder = 301;

        BuildingProgressBar progressBar = root.AddComponent<BuildingProgressBar>();
        progressBar.Initialize(
    instance,
    productionSystem,
    fill.transform,
    backgroundRenderer,
    fillRenderer
);
    }

}
using UnityEngine;

public sealed class BuildingProgressBar : MonoBehaviour
{
    [SerializeField] private BuildingInstance building;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private Transform fill;
    [SerializeField] private Renderer backgroundRenderer;
    [SerializeField] private Renderer fillRenderer;

    public void Initialize(
    BuildingInstance targetBuilding,
    ProductionSystem targetProductionSystem,
    Transform targetFill,
    Renderer targetBackgroundRenderer,
    Renderer targetFillRenderer
)
    {
        building = targetBuilding;
        productionSystem = targetProductionSystem;
        fill = targetFill;
        backgroundRenderer = targetBackgroundRenderer;
        fillRenderer = targetFillRenderer;
    }

    private void Awake()
    {
        if (building == null)
            building = GetComponentInParent<BuildingInstance>();

        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
    }

    private void Update()
    {
        if (building == null || productionSystem == null || fill == null)
            return;

        float progress = productionSystem.GetProcessProgress(building);
        fill.localScale = new Vector3(progress, 1f, 1f);
        fill.localPosition = new Vector3((progress - 1f) * 0.5f, 0f, -0.01f);

        bool visible = building.Definition != null && building.Definition.processInterval.ToMinutes() > 0;

        if (backgroundRenderer != null)
            backgroundRenderer.enabled = visible;

        if (fillRenderer != null)
            fillRenderer.enabled = visible;
    }
}
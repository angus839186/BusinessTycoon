using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public sealed class BuildingTooltipUI : MonoBehaviour
{
    [SerializeField] private RectTransform tooltipRoot;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 screenOffset = new Vector2(16f, -16f);

    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private bool showDebugDetails = false;
    [SerializeField] private int maxInventoryLines = 3;

    private readonly StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
    }

    private void Update()
    {
        if (Mouse.current == null || Camera.main == null)
        {
            Hide();
            return;
        }

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            Hide();
            return;
        }

        BuildingInstance building = hit.collider.GetComponent<BuildingInstance>();

        if (building == null)
        {
            Hide();
            return;
        }



        Show(building, screenPosition);
    }

    private void Show(BuildingInstance building, Vector2 screenPosition)
    {
        if (tooltipRoot == null || tooltipText == null)
            return;

        BuildingDefinition definition = building.Definition;

        if (definition == null)
        {
            Hide();
            return;
        }

        builder.Clear();
        builder.AppendLine(definition.displayName);

        if (productionSystem == null)
        {
            builder.AppendLine("No ProductionSystem");
        }
        else if (productionSystem.IsWaitingForInput(building))
        {
            builder.AppendLine("Waiting Input");
        }
        else
        {
            float progress = productionSystem.GetProcessProgress(building);
            builder.AppendLine($"Working {progress:P0}");
        }

        AppendCompactInventory(building);

        if (showDebugDetails && productionSystem != null)
        {
            LocationValueEvaluation evaluation =
                productionSystem.GetEconomyEvaluation(building);

            LocationValueModifiers modifiers = evaluation.modifiers;

            builder.AppendLine($"Area: {evaluation.areaName}");
            builder.AppendLine($"Population: {evaluation.population01:0.##}");
            builder.AppendLine($"Cost x{modifiers.constructionCostMultiplier:0.##}");
            builder.AppendLine($"Maint x{modifiers.maintenanceCostMultiplier:0.##}");
            builder.AppendLine($"Sale x{modifiers.commercialSalePriceMultiplier:0.##}");
            builder.AppendLine($"Resource Speed x{modifiers.resourceProductionSpeedMultiplier:0.##}");
        }

        tooltipText.text = builder.ToString();
        tooltipRoot.position = screenPosition + screenOffset;
        tooltipRoot.gameObject.SetActive(true);
    }

    private void Hide()
    {
        if (tooltipRoot != null && tooltipRoot.gameObject.activeSelf)
            tooltipRoot.gameObject.SetActive(false);
    }

    private void AppendCompactInventory(BuildingInstance building)
    {
        if (building.Inventory.Resources.Count == 0)
        {
            builder.AppendLine("Inventory: Empty");
            return;
        }

        int shown = 0;

        foreach (ResourceStack stack in building.Inventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
                continue;

            builder.AppendLine($"{stack.resource.displayName} x{stack.amount}");
            shown++;

            if (shown >= maxInventoryLines)
                break;
        }

        if (shown == 0)
            builder.AppendLine("Inventory: Empty");
    }
}
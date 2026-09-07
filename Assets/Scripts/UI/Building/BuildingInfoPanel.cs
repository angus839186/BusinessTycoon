using System.Text;
using TMPro;
using UnityEngine;

public sealed class BuildingInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private ProductionSystem productionSystem;

    private readonly StringBuilder builder = new StringBuilder();
    private BuildingInstance selectedBuilding;

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
    }

    private void OnEnable()
    {
        if (productionSystem != null)
        {
            productionSystem.DataChanged += Refresh;
            productionSystem.FinanceChanged += Refresh;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
        {
            productionSystem.DataChanged -= Refresh;
            productionSystem.FinanceChanged -= Refresh;
        }
    }
    public bool IsShowing(BuildingInstance building)
    {
        return selectedBuilding == building;
    }

    public void SetBuilding(BuildingInstance building)
    {
        selectedBuilding = building;
        Refresh();
    }

    public void ClearSelection()
    {
        selectedBuilding = null;

        if (outputText != null)
            outputText.text = "Select a building";
    }

    private void Refresh()
    {
        if (outputText == null)
            return;

        if (selectedBuilding == null)
        {
            outputText.text = "Select a building";
            return;
        }

        BuildingDefinition definition = selectedBuilding.Definition;

        if (definition == null)
            return;

        builder.Clear();
        builder.AppendLine(definition.displayName);
        builder.AppendLine($"Id: {selectedBuilding.InstanceId}");
        builder.AppendLine($"Type: {definition.category}");
        builder.AppendLine($"Lon: {selectedBuilding.Longitude:F6}");
        builder.AppendLine($"Lat: {selectedBuilding.Latitude:F6}");

        if (productionSystem != null)
        {
            float progress = productionSystem.GetProcessProgress(selectedBuilding);
            builder.AppendLine($"Progress: {progress:P0}");

            LocationValueEvaluation evaluation =
                productionSystem.GetEconomyEvaluation(selectedBuilding);

            LocationValueModifiers modifiers = evaluation.modifiers;

            builder.AppendLine($"Area: {evaluation.areaName}");
            builder.AppendLine($"Population: {evaluation.population01:0.##}");
            builder.AppendLine($"Cost x{modifiers.constructionCostMultiplier:0.##}");
            builder.AppendLine($"Maint x{modifiers.maintenanceCostMultiplier:0.##}");
            builder.AppendLine($"Sale x{modifiers.commercialSalePriceMultiplier:0.##}");
            builder.AppendLine($"Resource Speed x{modifiers.resourceProductionSpeedMultiplier:0.##}");
        }

        builder.AppendLine("Inventory:");

        foreach (ResourceStack stack in selectedBuilding.Inventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
                continue;

            builder.AppendLine($"- {stack.resource.displayName}: {stack.amount}");
        }

        outputText.text = builder.ToString();
    }
}
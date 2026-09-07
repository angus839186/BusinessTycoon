using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
public sealed class ResourceSummaryPanel : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TextMeshProUGUI outputText;
    private readonly StringBuilder builder = new StringBuilder();
    private readonly Dictionary<ResourceDefinition, int> totals =
        new Dictionary<ResourceDefinition, int>();

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
    }
    private void OnEnable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged -= Refresh;
    }

    private void Refresh()
    {
        if (outputText == null)
            return;

        builder.Clear();
        totals.Clear();

        if (productionSystem == null)
        {
            builder.AppendLine("ProductionSystem missing");
            outputText.text = builder.ToString();
            return;
        }
        foreach (ResourceStack stack in productionSystem.GlobalInventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
                continue;

            if (!totals.ContainsKey(stack.resource))
                totals.Add(stack.resource, 0);

            totals[stack.resource] += stack.amount;
        }

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null)
                continue;

            foreach (ResourceStack stack in building.Inventory.Resources)
            {
                if (stack == null || stack.resource == null || stack.amount <= 0)
                    continue;

                if (!totals.ContainsKey(stack.resource))
                    totals.Add(stack.resource, 0);

                totals[stack.resource] += stack.amount;
            }
        }

        builder.AppendLine("Resource Summary");

        if (totals.Count == 0)
        {
            builder.AppendLine("- Empty");
        }
        else
        {
            foreach (KeyValuePair<ResourceDefinition, int> pair in totals)
            {
                string name = string.IsNullOrWhiteSpace(pair.Key.displayName)
                    ? pair.Key.resourceId
                    : pair.Key.displayName;

                builder.AppendLine($"- {name}: {pair.Value}");
            }
        }

        outputText.text = builder.ToString();
    }
}
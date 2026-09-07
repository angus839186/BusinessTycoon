using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;
public sealed class EconomyDebugPanel : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private TransportSystem transport;
    private readonly StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();
    }
    private void OnEnable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged += Refresh;

        if (transport != null)
        {
            transport.RoutesChanged += Refresh;
            transport.TransportChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged -= Refresh;

        if (transport != null)
        {
            transport.RoutesChanged -= Refresh;
            transport.TransportChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        int buildingCount = productionSystem.Buildings.Count;
        int routeCount = transport != null ? transport.Routes.Count : 0;
        int totalAssetValue = CalculateTotalAssetValue();
        builder.Clear();

        builder.AppendLine($"建築數量: {buildingCount}");
        builder.AppendLine($"運輸路線數量: {routeCount}");
        builder.AppendLine($"總資產: {totalAssetValue}");

        outputText.text = builder.ToString();
    }
    private int CalculateTotalAssetValue()
    {
        if (productionSystem == null)
            return 0;

        int total = 0;

        foreach (ResourceStack stack in productionSystem.GlobalInventory.Resources)
            total += CalculateResourceStackValue(stack);

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null || building.Definition == null)
                continue;

            total += CalculateResourceAmountsValue(building.ConstructionCostSnapshot);

            foreach (ResourceStack stack in building.Inventory.Resources)
                total += CalculateResourceStackValue(stack);
        }

        return total;
    }

    private int CalculateResourceStackValue(ResourceStack stack)
    {
        if (stack == null || stack.resource == null || stack.amount <= 0)
            return 0;

        if (stack.resource.basePrice <= 0)
            return stack.amount;

        return stack.resource.basePrice * stack.amount;
    }

    private int CalculateResourceAmountsValue(IReadOnlyList<ResourceAmount> amounts)
    {
        if (amounts == null)
            return 0;

        int total = 0;

        foreach (ResourceAmount amount in amounts)
        {
            if (amount == null || amount.resource == null || amount.amount <= 0)
                continue;

            if (amount.resource.basePrice <= 0)
                total += amount.amount;
            else
                total += amount.resource.basePrice * amount.amount;
        }

        return total;
    }
}
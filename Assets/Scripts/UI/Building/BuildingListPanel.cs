using System.Text;
using UnityEngine;
using TMPro;

public sealed class BuildingListPanel : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TextMeshProUGUI outputText;

    [SerializeField] private GameClock gameClock;
    private readonly StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
        if (gameClock == null)
            gameClock = FindFirstObjectByType<GameClock>();
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

        if (productionSystem == null)
        {
            builder.AppendLine("ProductionSystem missing");
            outputText.text = builder.ToString();
            return;
        }

        builder.AppendLine("Building List");

        for (int i = 0; i < productionSystem.Buildings.Count; i++)
        {
            BuildingInstance building = productionSystem.Buildings[i];

            if (building == null || building.Definition == null)
                continue;

            BuildingDefinition definition = building.Definition;

            string mainResourceText = GetMainInventoryText(building);

            builder.AppendLine($"{definition.displayName} | {mainResourceText}");
        }

        outputText.text = builder.ToString();
    }

    private string GetMainInventoryText(BuildingInstance building)
    {
        if (building.Inventory.Resources.Count == 0)
            return "Inventory: Empty";

        ResourceStack firstStack = building.Inventory.Resources[0];

        if (firstStack == null || firstStack.resource == null)
            return "Inventory: Empty";

        return $"Inventory: {firstStack.resource.displayName} x{firstStack.amount}";
    }
}
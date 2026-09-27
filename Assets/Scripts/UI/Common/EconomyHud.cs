using TMPro;
using UnityEngine;

public sealed class EconomyHud : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TransportSystem transportSystem;

    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI expenseText;
    [SerializeField] private TextMeshProUGUI buildingCountText;
    [SerializeField] private TextMeshProUGUI routeCountText;
    [SerializeField] private TextMeshProUGUI totalAssetText;

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (transportSystem == null)
            transportSystem = FindFirstObjectByType<TransportSystem>();
    }

    private void OnEnable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged += Refresh;

        if (transportSystem != null)
            transportSystem.RoutesChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged -= Refresh;

        if (transportSystem != null)
            transportSystem.RoutesChanged -= Refresh;
    }

    private void Refresh()
    {
        MonthlyFinanceRecord currentMonth = GetCurrentMonth();

        SetText(incomeText, currentMonth?.income ?? 0);
        SetText(expenseText, currentMonth?.expense ?? 0);

        SetText(
            buildingCountText,
            productionSystem != null
                ? productionSystem.Buildings.Count
                : 0
        );

        SetText(
            routeCountText,
            transportSystem != null
                ? transportSystem.Routes.Count
                : 0
        );

        if (totalAssetText != null)
        {
            totalAssetText.text =
                EconomyValueCalculator
                    .CalculateTotalAssetValue(productionSystem)
                    .ToString("N0");
        }
    }

    private MonthlyFinanceRecord GetCurrentMonth()
    {
        if (
            productionSystem == null ||
            productionSystem.MonthlyFinanceRecords.Count == 0
        )
        {
            return null;
        }

        int lastIndex =
            productionSystem.MonthlyFinanceRecords.Count - 1;

        return productionSystem.MonthlyFinanceRecords[lastIndex];
    }

    private static void SetText(
        TextMeshProUGUI target,
        int value
    )
    {
        if (target != null)
            target.text = value.ToString("N0");
    }
}
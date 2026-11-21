using TMPro;
using UnityEngine;

public sealed class EconomyHud : MonoBehaviour
{
    [SerializeField] private MarketSystem marketSystem;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TransportSystem transportSystem;

    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI expenseText;
    [SerializeField] private TextMeshProUGUI buildingCountText;
    [SerializeField] private TextMeshProUGUI routeCountText;
    [SerializeField] private TextMeshProUGUI totalAssetText;

    private bool initialized;

    private bool HasRequiredReferences =>
        productionSystem != null && transportSystem != null &&
        incomeText != null && expenseText != null && buildingCountText != null &&
        routeCountText != null && totalAssetText != null;

    private void Awake()
    {
        if (marketSystem == null)
            marketSystem = FindFirstObjectByType<MarketSystem>();

        if (!HasRequiredReferences)
        {
            Debug.LogError("EconomyHud 缺少必要參考。", this);
            enabled = false;
            return;
        }

        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        productionSystem.DataChanged += Refresh;
        transportSystem.RoutesChanged += Refresh;

        if (marketSystem != null)
            marketSystem.PricesChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        productionSystem.DataChanged -= Refresh;
        transportSystem.RoutesChanged -= Refresh;

        if (marketSystem != null)
            marketSystem.PricesChanged -= Refresh;
    }

    private void Refresh()
    {
        MonthlyFinanceRecord currentMonth = GetCurrentMonth();

        incomeText.text = (currentMonth?.income ?? 0).ToString("N0");
        expenseText.text = (currentMonth?.expense ?? 0).ToString("N0");
        buildingCountText.text = productionSystem.Buildings.Count.ToString("N0");
        routeCountText.text = transportSystem.Routes.Count.ToString("N0");
        totalAssetText.text = AssetValueCalculator.CalculateTotalAssetValue(productionSystem, marketSystem).ToString("N0");
    }

    private MonthlyFinanceRecord GetCurrentMonth()
    {
        int count = productionSystem.MonthlyFinanceRecords.Count;
        return count > 0 ? productionSystem.MonthlyFinanceRecords[count - 1] : null;
    }
}
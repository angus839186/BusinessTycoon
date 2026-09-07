using System.Text;
using TMPro;
using UnityEngine;

public sealed class FinanceSummaryPanel : MonoBehaviour
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
            productionSystem.FinanceChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
            productionSystem.FinanceChanged -= Refresh;
    }

    public void Refresh()
    {
        if (outputText == null)
            return;

        builder.Clear();

        if (productionSystem == null)
        {
            outputText.text = "ProductionSystem missing";
            return;
        }
        int currentMonth = gameClock != null ? gameClock.Month : 1;

        int monthlyIncome = 0;
        int monthlyExpense = 0;
        int totalIncome = 0;
        int totalExpense = 0;

        foreach (IncomeRecord record in productionSystem.IncomeRecords)
        {
            totalIncome += record.income;

            if (gameClock != null && GetMonth(record.gameMinutes) == currentMonth)
                monthlyIncome += record.income;
        }

        foreach (ExpenseRecord record in productionSystem.ExpenseRecords)
        {
            totalExpense += record.expense;

            if (gameClock != null && GetMonth(record.gameMinutes) == currentMonth)
                monthlyExpense += record.expense;
        }

        builder.Append(
            $"月收入: {monthlyIncome}  月支出: {monthlyExpense}  月結: {monthlyIncome - monthlyExpense}  " +
            $"總收入: {totalIncome}  總支出: {totalExpense}  現金流: {totalIncome - totalExpense}"
        );
        outputText.text = builder.ToString();
    }
    private int GetMonth(double gameMinutes)
    {
        int daysPerMonth = 30;
        return (int)(gameMinutes / (daysPerMonth * 24 * 60)) + 1;
    }
}
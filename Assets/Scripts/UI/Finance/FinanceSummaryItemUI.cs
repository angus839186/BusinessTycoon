using TMPro;
using UnityEngine;
public sealed class FinanceSummaryItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI monthText;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI expenseText;
    [SerializeField] private TextMeshProUGUI netText;

    public void SetData(MonthlyFinanceRecord record)
    {
        if (record == null)
            return;

        if (monthText != null)
            monthText.text = $"第 {record.month} 月";

        if (incomeText != null)
            incomeText.text = record.income.ToString();

        if (expenseText != null)
            expenseText.text = record.expense.ToString();

        if (netText != null)
            netText.text = record.Net.ToString();
    }
}
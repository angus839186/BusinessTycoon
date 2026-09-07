public sealed class ExpenseRecord
{
    public string sourceName;
    public string expenseType;
    public string paidResources;
    public int moneyAfterExpense;
    public int expense;
    public double gameMinutes;

    public ExpenseRecord(
    string sourceName,
    string expenseType,
    string paidResources,
    int expense,
    int moneyAfterExpense,
    double gameMinutes
)
    {
        this.sourceName = sourceName;
        this.expenseType = expenseType;
        this.paidResources = paidResources;
        this.expense = expense;
        this.moneyAfterExpense = moneyAfterExpense;
        this.gameMinutes = gameMinutes;
    }
}
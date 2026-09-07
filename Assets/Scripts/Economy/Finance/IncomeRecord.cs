public sealed class IncomeRecord
{
    public string buildingName;
    public string soldResources;
    public int income;
    public int moneyAfterSale;
    public double gameMinutes;

    public IncomeRecord(
        string buildingName,
        string soldResources,
        int income,
        int moneyAfterSale,
        double gameMinutes
    )
    {
        this.buildingName = buildingName;
        this.soldResources = soldResources;
        this.income = income;
        this.moneyAfterSale = moneyAfterSale;
        this.gameMinutes = gameMinutes;
    }
}
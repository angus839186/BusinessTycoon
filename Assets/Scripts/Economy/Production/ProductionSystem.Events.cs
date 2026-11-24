using System;
public sealed partial class ProductionSystem
{
    public event Action DataChanged;
    public event Action FinanceChanged;
    public event Action BuildingsChanged;
    private void MarkDataChanged()
    {
        if (isRestoringState)
            return;

        DataChanged?.Invoke();
    }

    private void MarkFinanceChanged()
    {
        if (isRestoringState)
            return;

        FinanceChanged?.Invoke();
        MarkDataChanged();
    }
    private void MarkBuildingsChanged()
    {
        if (isRestoringState)
            return;

        BuildingsChanged?.Invoke();
        MarkDataChanged();
    }

}
using UnityEngine;

public sealed class FinanceSummaryPanel : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private Transform listRoot;
    [SerializeField] private FinanceSummaryItemUI itemPrefab;
    [SerializeField] private GameObject emptyText;

    private ReusableUIList<FinanceSummaryItemUI> itemList;

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();
        if (itemPrefab != null && listRoot != null)
        {
            itemList = new ReusableUIList<FinanceSummaryItemUI>(
                itemPrefab,
                listRoot
            );
        }
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
        int recordCount = productionSystem != null
            ? productionSystem.MonthlyFinanceRecords.Count
            : 0;

        if (itemList != null)
        {
            for (int i = 0; i < recordCount; i++)
            {
                FinanceSummaryItemUI item =
                    itemList.GetOrCreate(i);

                if (item == null)
                    continue;

                int recordIndex = recordCount - 1 - i;

                item.SetData(
                    productionSystem.MonthlyFinanceRecords[
                        recordIndex
                    ]
                );
            }

            itemList.HideFrom(recordCount);
        }

        if (emptyText != null)
            emptyText.SetActive(recordCount == 0);
    }
}
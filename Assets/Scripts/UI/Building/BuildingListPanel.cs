using UnityEngine;

public sealed class BuildingListPanel : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;

    [SerializeField] private Transform listRoot;
    [SerializeField] private BuildingListItemUI itemPrefab;
    [SerializeField] private GameObject emptyText;

    private ReusableUIList<BuildingListItemUI> itemList;

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (itemPrefab != null && listRoot != null)
        {
            itemList = new ReusableUIList<BuildingListItemUI>(
                itemPrefab,
                listRoot
            );
        }
    }

    private void OnEnable()
    {
        if (productionSystem != null)
            productionSystem.BuildingsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (productionSystem != null)
            productionSystem.BuildingsChanged -= Refresh;
    }

    private void Refresh()
    {
        if (productionSystem == null || itemList == null)
            return;

        int itemIndex = 0;

        foreach (BuildingInstance building in productionSystem.Buildings)
        {
            if (building == null || building.Definition == null)
                continue;

            BuildingListItemUI item =
                itemList.GetOrCreate(itemIndex);

            if (item == null)
                continue;

            item.Initialize(building);
            itemIndex++;
        }

        itemList.HideFrom(itemIndex);

        if (emptyText != null)
            emptyText.SetActive(itemIndex == 0);
    }
}
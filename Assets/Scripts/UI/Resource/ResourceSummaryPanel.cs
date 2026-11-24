using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class ResourceSummaryPanel : MonoBehaviour
{
    [SerializeField] private MarketSystem marketSystem;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private GameDefinitionCatalog definitionCatalog;

    [SerializeField] private Transform listRoot;
    [SerializeField] private ResourceSummaryItem itemPrefab;
    [SerializeField] private GameObject emptyText;



    private readonly Dictionary<
        ResourceDefinition,
        ResourceSummaryItem
    > items = new Dictionary<
        ResourceDefinition,
        ResourceSummaryItem
    >();

    private void Awake()
    {
        if (marketSystem == null)
        {
            marketSystem =
                FindFirstObjectByType<MarketSystem>();
        }
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (definitionCatalog == null)
            definitionCatalog =
                FindFirstObjectByType<GameDefinitionCatalog>();
    }

    private void OnEnable()
    {
        if (marketSystem != null)
        {
            marketSystem.PricesChanged += RefreshAmounts;
        }
        if (productionSystem != null)
            productionSystem.DataChanged += RefreshAmounts;

        EnsureItemsCreated();
        RefreshAmounts();
    }

    private void OnDisable()
    {
        if (marketSystem != null)
        {
            marketSystem.PricesChanged -= RefreshAmounts;
        }
        if (productionSystem != null)
            productionSystem.DataChanged -= RefreshAmounts;
    }

    private void EnsureItemsCreated()
    {
        if (
            items.Count > 0 ||
            definitionCatalog == null ||
            listRoot == null ||
            itemPrefab == null
        )
        {
            return;
        }

        List<ResourceDefinition> definitions =
            new List<ResourceDefinition>(
                definitionCatalog.ResourceDefinitions
            );

        definitions.Sort(CompareDefinitions);

        foreach (ResourceDefinition definition in definitions)
        {
            if (definition == null)
                continue;

            ResourceSummaryItem item =
                Instantiate(itemPrefab, listRoot);

            item.gameObject.SetActive(true);
            item.Initialize(definition, marketSystem);
            items.Add(definition, item);
        }

        if (emptyText != null)
            emptyText.SetActive(items.Count == 0);
    }

    private static int CompareDefinitions(
        ResourceDefinition left,
        ResourceDefinition right
    )
    {
        string leftName = left != null ? left.displayName : "";
        string rightName = right != null ? right.displayName : "";

        return string.Compare(
            leftName,
            rightName,
            StringComparison.Ordinal
        );
    }
}
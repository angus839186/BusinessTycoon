using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ResourceSummaryPanel : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private GameDefinitionCatalog definitionCatalog;

    [SerializeField] private Transform listRoot;
    [SerializeField] private ResourceSummaryItemUI itemPrefab;
    [SerializeField] private GameObject emptyText;

    private readonly Dictionary<ResourceDefinition, int> totals =
        new Dictionary<ResourceDefinition, int>();

    private readonly Dictionary<
        ResourceDefinition,
        ResourceSummaryItemUI
    > items = new Dictionary<
        ResourceDefinition,
        ResourceSummaryItemUI
    >();

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (definitionCatalog == null)
            definitionCatalog =
                FindFirstObjectByType<GameDefinitionCatalog>();
    }

    private void OnEnable()
    {
        if (productionSystem != null)
            productionSystem.DataChanged += RefreshAmounts;

        EnsureItemsCreated();
        RefreshAmounts();
    }

    private void OnDisable()
    {
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

            ResourceSummaryItemUI item =
                Instantiate(itemPrefab, listRoot);

            item.gameObject.SetActive(true);
            item.Initialize(definition);
            items.Add(definition, item);
        }

        if (emptyText != null)
            emptyText.SetActive(items.Count == 0);
    }

    private void RefreshAmounts()
    {
        totals.Clear();

        foreach (ResourceDefinition definition in items.Keys)
            totals.Add(definition, 0);

        if (productionSystem != null)
        {
            AddInventory(productionSystem.GlobalInventory);

            foreach (
                BuildingInstance building
                in productionSystem.Buildings
            )
            {
                if (building != null)
                    AddInventory(building.Inventory);
            }
        }

        foreach (
            KeyValuePair<ResourceDefinition, ResourceSummaryItemUI>
            pair in items
        )
        {
            int amount = totals.TryGetValue(
                pair.Key,
                out int total
            )
                ? total
                : 0;

            pair.Value.SetAmount(amount);
        }
    }

    private void AddInventory(BuildingInventory inventory)
    {
        if (inventory == null)
            return;

        foreach (ResourceStack stack in inventory.Resources)
        {
            if (
                stack == null ||
                stack.resource == null ||
                stack.amount <= 0
            )
            {
                continue;
            }

            if (!totals.ContainsKey(stack.resource))
                totals.Add(stack.resource, 0);

            totals[stack.resource] += stack.amount;
        }
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
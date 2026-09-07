using UnityEngine;
using System.Collections.Generic;
using System;

public sealed partial class ProductionSystem : MonoBehaviour
{
    [SerializeField] private GameRules gameRules;
    [SerializeField] private ResourceAmount[] startingResources;
    [SerializeField] private bool logProduction = true;
    [SerializeField] private int maxIncomeRecords = 20;
    [SerializeField] private ResourceDefinition moneyResource;
    [SerializeField] private GameClock gameClock;
    [SerializeField] private LocationValueEvaluator locationEconomyEvaluator;
    [SerializeField] private int maxExpenseRecords = 50;
    private readonly List<BuildingInstance> buildings = new List<BuildingInstance>();
    public IReadOnlyList<BuildingInstance> Buildings => buildings;

    private readonly List<IncomeRecord> incomeRecords = new List<IncomeRecord>();

    public IReadOnlyList<IncomeRecord> IncomeRecords => incomeRecords;

    private readonly BuildingInventory globalInventory = new BuildingInventory();

    public BuildingInventory GlobalInventory => globalInventory;


    private readonly List<ExpenseRecord> expenseRecords = new List<ExpenseRecord>();

    public IReadOnlyList<ExpenseRecord> ExpenseRecords => expenseRecords;
    public event Action DataChanged;
    public event Action FinanceChanged;

    private void Start()
    {
        if (gameRules == null)
            gameRules = FindFirstObjectByType<GameRules>();
        if (gameClock == null)
            gameClock = FindFirstObjectByType<GameClock>();
        if (locationEconomyEvaluator == null)
            locationEconomyEvaluator = FindFirstObjectByType<LocationValueEvaluator>();
        foreach (ResourceAmount resourceAmount in startingResources)
        {
            if (
                resourceAmount == null ||
                resourceAmount.resource == null ||
                resourceAmount.amount <= 0
            )
                continue;

            globalInventory.Add(resourceAmount.resource, resourceAmount.amount);
        }
        MarkDataChanged();
        BuildingInstance[] existingBuildings = FindObjectsByType<BuildingInstance>(
            FindObjectsSortMode.None
        );

        foreach (BuildingInstance building in existingBuildings)
            Register(building);
    }
    private void Update()
    {
        for (int i = buildings.Count - 1; i >= 0; i--)
        {
            BuildingInstance building = buildings[i];

            if (building == null)
            {
                buildings.RemoveAt(i);
                continue;
            }

            TickBuilding(building, Time.deltaTime);
        }
    }

}
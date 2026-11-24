using UnityEngine;

public sealed partial class ProductionSystem : MonoBehaviour
{
    [SerializeField] private MarketSystem marketSystem;
    [SerializeField] private GameRules gameRules;
    [SerializeField] private ResourceAmount[] startingResources;
    [SerializeField] private bool logProduction = true;
    [SerializeField] private int maxIncomeRecords = 20;
    [SerializeField] private ResourceDefinition moneyResource;
    [SerializeField] private GameClock gameClock;
    [SerializeField] private LocationValueEvaluator locationEconomyEvaluator;
    [SerializeField] private int maxExpenseRecords = 50;

    private void Awake()
    {
        if (!ValidateDependencies())
            enabled = false;
    }

    private bool ValidateDependencies()
    {
        if (gameRules == null)
            return LogMissingDependency(nameof(GameRules));

        if (moneyResource == null)
            return LogMissingDependency("a money resource");

        if (gameClock == null)
            return LogMissingDependency(nameof(GameClock));

        if (locationEconomyEvaluator == null)
            return LogMissingDependency(nameof(LocationValueEvaluator));

        return true;
    }

    private bool LogMissingDependency(string dependency)
    {
        Debug.LogError($"{nameof(ProductionSystem)} requires {dependency}.", this);
        return false;
    }

    private void Start()
    {
        InitializeFinance();
        InitializeStartingResources();
        RegisterExistingBuildings();
    }
    private void Update()
    {
        TickBuildings();
    }
    private void OnDestroy()
    {
        ShutdownFinance();
    }


}
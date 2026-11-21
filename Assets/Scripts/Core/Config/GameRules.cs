using System;
using System.IO;
using UnityEngine;

public sealed class GameRules : MonoBehaviour
{
    [SerializeField] private string configPath = "Config/game_rules.json";

    private GameRulesConfig config = CreateFallbackConfig();

    public GameRulesConfig Config => config;
    public float DemolitionCostRate => Mathf.Clamp01(config.demolitionCostRate);
    public bool AutoSaveEnabled => config.autoSaveEnabled;
    public float AutoSaveIntervalMinutes => Mathf.Max(0.1f, config.autoSaveIntervalMinutes);
    public float ConstructionCostMultiplier => Mathf.Max(0.01f, config.constructionCostMultiplier);
    public float MaintenanceCostMultiplier => Mathf.Max(0.01f, config.maintenanceCostMultiplier);
    public float CommercialSalePriceMultiplier => Mathf.Max(0.01f, config.commercialSalePriceMultiplier);
    public float ResourceProductionSpeedMultiplier => Mathf.Max(0.01f, config.resourceProductionSpeedMultiplier);
    public float TransportIntervalMultiplier => Mathf.Max(0.01f, config.transportIntervalMultiplier);
    public double MaintenanceIntervalMinutes => config.buildingMaintenanceInterval.ToMinutes();

    private void Awake()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        config = CreateFallbackConfig();

        string fullPath = Path.Combine(Application.streamingAssetsPath, configPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Game rules config missing: {fullPath}", this);
            return;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            JsonUtility.FromJsonOverwrite(json, config);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private static GameRulesConfig CreateFallbackConfig()
    {
        return new GameRulesConfig
        {
            buildingMaintenanceInterval = new GameTimeInterval { months = 0, days = 0, hours = 1, minutes = 0 },
            demolitionCostRate = 0.2f,
            constructionCostMultiplier = 1f,
            maintenanceCostMultiplier = 1f,
            commercialSalePriceMultiplier = 1f,
            resourceProductionSpeedMultiplier = 1f,
            transportIntervalMultiplier = 1f,
            autoSaveEnabled = true,
            autoSaveIntervalMinutes = 5f
        };
    }
}
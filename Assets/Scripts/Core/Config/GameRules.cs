using System.IO;
using UnityEngine;

public sealed class GameRules : MonoBehaviour
{
    [SerializeField] private string configPath = "Config/game_rules.json";

    private GameRulesConfig config;

    public GameRulesConfig Config => config;

    private void Awake()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        string fullPath = Path.Combine(
            Application.streamingAssetsPath,
            configPath
        );

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Game rules config missing: {fullPath}");
            config = CreateFallbackConfig();
            return;
        }

        string json = File.ReadAllText(fullPath);
        config = JsonUtility.FromJson<GameRulesConfig>(json);

        if (config == null)
            config = CreateFallbackConfig();
    }

    private GameRulesConfig CreateFallbackConfig()
    {
        return new GameRulesConfig
        {
            buildingMaintenanceInterval = new GameTimeInterval
            {
                months = 0,
                days = 0,
                hours = 1,
                minutes = 0
            },
            demolitionCostRate = 0.2f
        };
    }
    public float DemolitionCostRate
    {
        get
        {
            if (config == null)
                return 0.2f;

            return Mathf.Clamp01(config.demolitionCostRate);
        }
    }
    public float ConstructionCostMultiplier =>
    config != null ? Mathf.Max(0.01f, config.constructionCostMultiplier) : 1f;

    public float MaintenanceCostMultiplier =>
        config != null ? Mathf.Max(0.01f, config.maintenanceCostMultiplier) : 1f;

    public float CommercialSalePriceMultiplier =>
        config != null ? Mathf.Max(0.01f, config.commercialSalePriceMultiplier) : 1f;

    public float ResourceProductionSpeedMultiplier =>
        config != null ? Mathf.Max(0.01f, config.resourceProductionSpeedMultiplier) : 1f;

    public float TransportIntervalMultiplier =>
        config != null ? Mathf.Max(0.01f, config.transportIntervalMultiplier) : 1f;

}
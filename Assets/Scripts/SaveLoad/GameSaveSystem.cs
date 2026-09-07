using System.IO;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed partial class GameSaveSystem : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TransportSystem transportSystem;
    [SerializeField] private GameClock gameClock;
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private GameDefinitionCatalog definitionCatalog;

    public const string DefaultSaveFileName = "game_save.json";

    [SerializeField]
    private string saveFileName = DefaultSaveFileName;

    public static string DefaultSavePath =>
        Path.Combine(
            Application.persistentDataPath,
            DefaultSaveFileName
        );

    public static bool DefaultSaveExists =>
        File.Exists(DefaultSavePath);

    public string SavePath =>
        Path.Combine(Application.persistentDataPath, saveFileName);

    public bool HasSaveFile => File.Exists(SavePath);

    private void Awake()
    {
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (transportSystem == null)
            transportSystem = FindFirstObjectByType<TransportSystem>();

        if (gameClock == null)
            gameClock = FindFirstObjectByType<GameClock>();
        if (definitionCatalog == null)
            definitionCatalog = FindFirstObjectByType<GameDefinitionCatalog>();

        if (buildingPlacement == null)
            buildingPlacement = FindFirstObjectByType<BuildingPlacementController>();
    }
    private void Start()
    {
        if (GameLaunchRequest.ConsumeLoadRequest())
            LoadGame();
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        GameSaveData saveData = CreateSaveData();
        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(
            SavePath,
            json,
            new UTF8Encoding(false)
        );

        Debug.Log(
            $"Game saved: {SavePath}, " +
            $"Buildings={saveData.buildings.Count}, " +
            $"Routes={saveData.transportRoutes.Count}"
        );
    }
}
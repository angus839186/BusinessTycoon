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

    [SerializeField]
    private string activeSlotId = SaveSlots.DefaultId;

    public string ActiveSlotId => activeSlotId;

    public const string DefaultSaveFileName = "game_save.json";

    [SerializeField]
    private string saveFileName = DefaultSaveFileName;

    public static string DefaultSavePath =>
        Path.Combine(
            Application.persistentDataPath,
            DefaultSaveFileName
        );

    public static bool DefaultSaveExists =>
    SaveSlots.Exists(SaveSlots.DefaultId) ||
    File.Exists(DefaultSavePath);

    public string SavePath =>
        SaveSlots.GetPath(activeSlotId);

    private string LegacySavePath =>
        Path.Combine(
            Application.persistentDataPath,
            saveFileName
        );

    private string LoadPath
    {
        get
        {
            if (File.Exists(SavePath))
                return SavePath;

            if (
                activeSlotId == SaveSlots.DefaultId &&
                File.Exists(LegacySavePath)
            )
            {
                return LegacySavePath;
            }

            return SavePath;
        }
    }

    public bool HasSaveFile =>
        File.Exists(LoadPath);

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

        if (!SaveSlots.IsValidId(activeSlotId))
        {
            Debug.LogWarning(
                $"Invalid active slot id: {activeSlotId}. " +
                $"Using {SaveSlots.DefaultId}."
            );

            activeSlotId = SaveSlots.DefaultId;
        }
    }

    private void Start()
    {
        if (!GameLaunchRequest.ConsumeLoadRequest(out string slotId))
            return;

        if (!SetActiveSlot(slotId)) return;

        LoadGame();
    }

    public bool SetActiveSlot(string slotId)
    {
        if (!SaveSlots.IsValidId(slotId))
        {
            Debug.LogWarning(
                $"Invalid save slot id: {slotId}"
            );

            return false;
        }

        activeSlotId = slotId;
        return true;
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (!SaveSlots.IsValidId(activeSlotId))
        {
            Debug.LogError(
                $"Cannot save invalid slot: {activeSlotId}"
            );

            return;
        }

        SaveSlots.EnsureFolder();
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
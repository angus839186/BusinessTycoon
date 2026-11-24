public sealed partial class GameSaveSystem
{
    private const int CurrentSaveVersion = 2;

    private static bool TryMigrateSaveData(GameSaveData saveData)
    {
        if (saveData.saveVersion == 1)
        {
            if (saveData.buildings != null)
            {
                foreach (BuildingSaveData building in saveData.buildings)
                {
                    if (building != null)
                        building.isOperational = true;
                }
            }

            saveData.saveVersion = 2;
        }

        return saveData.saveVersion == CurrentSaveVersion;
    }

    public static bool CanLoadVersion(int version)
    {
        return version == 1 || version == CurrentSaveVersion;
    }
}
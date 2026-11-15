using UnityEngine;

public static class GameLaunchRequest
{
    private static bool shouldLoadSave;
    private static string requestedSlotId = SaveSlots.DefaultId;

    public static void RequestNewGame()
    {
        shouldLoadSave = false;
        requestedSlotId = SaveSlots.DefaultId;
    }

    public static void RequestContinueGame()
    {
        RequestContinueGame(SaveSlots.DefaultId);
    }

    public static bool RequestContinueGame(string slotId)
    {
        if (!SaveSlots.IsValidId(slotId))
        {
            Debug.LogWarning(
                $"Invalid save slot id: {slotId}"
            );

            return false;
        }

        requestedSlotId = slotId;
        shouldLoadSave = true;
        return true;
    }

    public static bool ConsumeLoadRequest()
    {
        return ConsumeLoadRequest(out _);
    }

    public static bool ConsumeLoadRequest(
        out string slotId
    )
    {
        bool result = shouldLoadSave;

        slotId = SaveSlots.IsValidId(requestedSlotId)
            ? requestedSlotId
            : SaveSlots.DefaultId;

        shouldLoadSave = false;
        requestedSlotId = SaveSlots.DefaultId;

        return result;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetState()
    {
        shouldLoadSave = false;
        requestedSlotId = SaveSlots.DefaultId;
    }
}
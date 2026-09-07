using UnityEngine;

public static class GameLaunchRequest
{
    private static bool shouldLoadSave;

    public static void RequestNewGame()
    {
        shouldLoadSave = false;
    }

    public static void RequestContinueGame()
    {
        shouldLoadSave = true;
    }

    public static bool ConsumeLoadRequest()
    {
        bool result = shouldLoadSave;
        shouldLoadSave = false;
        return result;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetState()
    {
        shouldLoadSave = false;
    }
}
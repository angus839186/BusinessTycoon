using TMPro;
using UnityEngine;

public sealed class GameClockPanel : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;
    [SerializeField] private TextMeshProUGUI outputText;

    private void Awake()
    {
        if (gameClock == null)
            gameClock = FindFirstObjectByType<GameClock>();
    }

    private void OnEnable()
    {
        if (gameClock != null)
        {
            gameClock.TimeChanged += Refresh;
            gameClock.SpeedChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (gameClock != null)
        {
            gameClock.TimeChanged -= Refresh;
            gameClock.SpeedChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        if (gameClock == null || outputText == null)
            return;

        outputText.text =
            $"{gameClock.GetDisplayText()}  x{gameClock.SpeedMultiplier:0.##}";
    }
}
using TMPro;
using UnityEngine;

public sealed class GameClockPanel : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameClock gameClock;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private TextMeshProUGUI speedText;

    private void Awake()
    {
        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();
        if (gameClock == null)
            gameClock = FindFirstObjectByType<GameClock>();
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.SpeedUpPressed += HandleSpeedUp;
            gameInput.SpeedDownPressed += HandleSpeedDown;
        }
        if (gameClock != null)
        {
            gameClock.TimeChanged += Refresh;
            gameClock.SpeedChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.SpeedUpPressed -= HandleSpeedUp;
            gameInput.SpeedDownPressed -= HandleSpeedDown;
        }
        if (gameClock != null)
        {
            gameClock.TimeChanged -= Refresh;
            gameClock.SpeedChanged -= Refresh;
        }
    }
    private void HandleSpeedUp()
    {
        if (gameClock != null)
            gameClock.IncreaseSpeed();
    }

    private void HandleSpeedDown()
    {
        if (gameClock != null)
            gameClock.DecreaseSpeed();
    }

    private void Refresh()
    {
        if (gameClock == null) return;

        if (outputText != null)
            outputText.text = gameClock.GetDisplayText();

        if (speedText != null)
            speedText.text = $"x{gameClock.SpeedMultiplier:0.##}";
    }
}
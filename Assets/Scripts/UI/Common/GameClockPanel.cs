using TMPro;
using UnityEngine;

public sealed class GameClockPanel : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameClock gameClock;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private TextMeshProUGUI speedText;

    private bool initialized;

    private bool HasRequiredReferences =>
        gameInput != null &&
        gameClock != null &&
        outputText != null &&
        speedText != null;

    private void Awake()
    {
        if (!HasRequiredReferences)
        {
            Debug.LogError("GameClockPanel 缺少必要參考。", this);
            enabled = false;
            return;
        }

        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        gameInput.SpeedUpPressed += HandleSpeedUp;
        gameInput.SpeedDownPressed += HandleSpeedDown;
        gameClock.TimeChanged += Refresh;
        gameClock.SpeedChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        gameInput.SpeedUpPressed -= HandleSpeedUp;
        gameInput.SpeedDownPressed -= HandleSpeedDown;
        gameClock.TimeChanged -= Refresh;
        gameClock.SpeedChanged -= Refresh;
    }

    private void HandleSpeedUp() => gameClock.IncreaseSpeed();

    private void HandleSpeedDown() => gameClock.DecreaseSpeed();

    private void Refresh()
    {
        outputText.text = gameClock.GetDisplayText();
        speedText.text = $"x{gameClock.SpeedMultiplier:0.##}";
    }
}
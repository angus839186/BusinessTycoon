using UnityEngine;
using System;

public sealed partial class GameClock : MonoBehaviour
{
    [SerializeField] private float gameMinutesPerRealSecond = 1f;
    [SerializeField] private int daysPerMonth = 30;
    [SerializeField] private int startYear = 2026;
    [SerializeField] private float minSpeedMultiplier = 0.25f;
    [SerializeField] private float maxSpeedMultiplier = 4f;
    [SerializeField] private float speedStep = 0.25f;

    private const int MonthsPerYear = 12;
    private float speedMultiplier = 1f;
    private int lastDisplayedMonth = 0;
    private int lastDisplayedMinute = -1;
    public float SpeedMultiplier => speedMultiplier;
    private double totalGameMinutes;
    public double TotalGameMinutes => totalGameMinutes;
    private int ValidDaysPerMonth => Mathf.Max(1, daysPerMonth);

    private double MinutesPerMonth =>
        ValidDaysPerMonth * 24d * 60d;

    public int MonthIndex =>
        (int)(totalGameMinutes / MinutesPerMonth);

    public int Year =>
        Mathf.Max(1, startYear) + MonthIndex / MonthsPerYear;

    public int Month =>
        MonthIndex % MonthsPerYear + 1;

    public int Day =>
        (int)(totalGameMinutes / (24 * 60)) % ValidDaysPerMonth + 1;

    public int Hour =>
        (int)(totalGameMinutes / 60) % 24;

    public int Minute =>
        (int)(totalGameMinutes % 60);
    public event Action MonthChanged;
    public event Action TimeChanged;
    public event Action SpeedChanged;

    private void Update()
    {
        totalGameMinutes +=
            Time.deltaTime *
            gameMinutesPerRealSecond *
            speedMultiplier;

        int currentMonth = MonthIndex;

        if (currentMonth != lastDisplayedMonth)
        {
            lastDisplayedMonth = currentMonth;
            MonthChanged?.Invoke();
        }

        int currentDisplayedMinute = (int)totalGameMinutes;

        if (currentDisplayedMinute == lastDisplayedMinute) return;

        lastDisplayedMinute = currentDisplayedMinute;
        TimeChanged?.Invoke();
    }


    public void IncreaseSpeed()
    {
        SetSpeedMultiplier(speedMultiplier + speedStep);
    }

    public void DecreaseSpeed()
    {
        SetSpeedMultiplier(speedMultiplier - speedStep);
    }

    public void SetNormalSpeed()
    {
        SetSpeedMultiplier(1f);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        float clamped = Mathf.Clamp(multiplier, minSpeedMultiplier, maxSpeedMultiplier);

        if (Mathf.Approximately(speedMultiplier, clamped)) return;

        speedMultiplier = clamped;
        SpeedChanged?.Invoke();
        TimeChanged?.Invoke();
    }
    public void RestoreState(
    double restoredGameMinutes,
    float restoredSpeedMultiplier)
    {
        if (
            double.IsNaN(restoredGameMinutes) ||
            double.IsInfinity(restoredGameMinutes)
        )
        {
            restoredGameMinutes = 0;
        }

        if (
            float.IsNaN(restoredSpeedMultiplier) ||
            float.IsInfinity(restoredSpeedMultiplier)
        )
        {
            restoredSpeedMultiplier = 1f;
        }

        totalGameMinutes = Math.Max(0, restoredGameMinutes);

        speedMultiplier = Mathf.Clamp(
            restoredSpeedMultiplier,
            minSpeedMultiplier,
            maxSpeedMultiplier
        );

        lastDisplayedMinute = (int)totalGameMinutes;
        lastDisplayedMonth = MonthIndex;

        SpeedChanged?.Invoke();
        TimeChanged?.Invoke();
    }
}
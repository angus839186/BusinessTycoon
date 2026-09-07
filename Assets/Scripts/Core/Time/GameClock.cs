using UnityEngine;
using System;

public sealed class GameClock : MonoBehaviour
{
    [SerializeField] private float gameMinutesPerRealSecond = 1f;
    [SerializeField] private int daysPerMonth = 30;

    [SerializeField] private float minSpeedMultiplier = 0.25f;
    [SerializeField] private float maxSpeedMultiplier = 4f;
    [SerializeField] private float speedStep = 0.25f;

    private float speedMultiplier = 1f;
    private int lastDisplayedMinute = -1;

    public event Action TimeChanged;
    public event Action SpeedChanged;

    public float SpeedMultiplier => speedMultiplier;

    private double totalGameMinutes;

    public double TotalGameMinutes => totalGameMinutes;

    public int Month => (int)(totalGameMinutes / (daysPerMonth * 24 * 60)) + 1;
    public int Day => (int)(totalGameMinutes / (24 * 60)) % daysPerMonth + 1;
    public int Hour => (int)(totalGameMinutes / 60) % 24;
    public int Minute => (int)(totalGameMinutes % 60);

    private void Update()
    {
        totalGameMinutes += Time.deltaTime * gameMinutesPerRealSecond * speedMultiplier;

        int currentDisplayedMinute = (int)totalGameMinutes;

        if (currentDisplayedMinute == lastDisplayedMinute)
            return;

        lastDisplayedMinute = currentDisplayedMinute;
        TimeChanged?.Invoke();
    }

    public string GetDisplayText()
    {
        return $"{Month:00}:{Day:00}:{Hour:00}:{Minute:00}";
    }
    public string FormatMinutes(double gameMinutes)
    {
        int month = (int)(gameMinutes / (daysPerMonth * 24 * 60)) + 1;
        int day = (int)(gameMinutes / (24 * 60)) % daysPerMonth + 1;
        int hour = (int)(gameMinutes / 60) % 24;
        int minute = (int)(gameMinutes % 60);

        return $"{month:00}:{day:00}:{hour:00}:{minute:00}";
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

        if (Mathf.Approximately(speedMultiplier, clamped))
            return;

        speedMultiplier = clamped;
        SpeedChanged?.Invoke();
        TimeChanged?.Invoke();
    }
    public void RestoreState(
    double restoredGameMinutes,
    float restoredSpeedMultiplier
)
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

        SpeedChanged?.Invoke();
        TimeChanged?.Invoke();
    }
}
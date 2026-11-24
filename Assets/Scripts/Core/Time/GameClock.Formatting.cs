using System;
using UnityEngine;

public sealed partial class GameClock
{
    public string GetDisplayText()
    {
        return
            $"{Year}年/{Month}月/{Day}日/" +
            $"{Hour}時/{Minute}分";
    }
    public string FormatMonth(int monthNumber)
    {
        int monthIndex =
            Math.Max(0, monthNumber - 1);

        int year =
            Mathf.Max(1, startYear) +
            monthIndex / MonthsPerYear;

        int month =
            monthIndex % MonthsPerYear + 1;

        return $"{year}年/{month}月";
    }
    public string FormatMinutes(double gameMinutes)
    {
        gameMinutes = Math.Max(0, gameMinutes);

        int monthIndex =
            (int)(gameMinutes / MinutesPerMonth);

        int year =
            Mathf.Max(1, startYear) +
            monthIndex / MonthsPerYear;

        int month =
            monthIndex % MonthsPerYear + 1;

        int day =
            (int)(gameMinutes / (24 * 60)) %
            ValidDaysPerMonth + 1;

        int hour =
            (int)(gameMinutes / 60) % 24;

        int minute =
            (int)(gameMinutes % 60);

        return
            $"{year}年/{month}月/{day}日/" +
            $"{hour}時/{minute}分";
    }
}
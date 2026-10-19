using System;

[Serializable]
public struct GameTimeInterval
{
    [GameDataColumn(MinValue = 0)]
    public int months;

    [GameDataColumn(MinValue = 0)]
    public int days;

    [GameDataColumn(MinValue = 0)]
    public int hours;

    [GameDataColumn(MinValue = 0)]
    public int minutes;

    public double ToMinutes(int daysPerMonth = 30)
    {
        return
            months * daysPerMonth * 24 * 60 +
            days * 24 * 60 +
            hours * 60 +
            minutes;
    }
}
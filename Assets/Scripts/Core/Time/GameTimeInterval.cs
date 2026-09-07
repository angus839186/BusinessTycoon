using System;

[Serializable]
public struct GameTimeInterval
{
    public int months;
    public int days;
    public int hours;
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
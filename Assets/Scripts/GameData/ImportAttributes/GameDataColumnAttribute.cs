using System;

[AttributeUsage(
    AttributeTargets.Field,
    Inherited = true,
    AllowMultiple = false
)]
public class GameDataColumnAttribute : Attribute
{
    public bool Required { get; set; } = true;
    public bool AllowEmpty { get; set; }

    public double MinValue { get; set; } = double.NaN;
    public double MaxValue { get; set; } = double.NaN;
}
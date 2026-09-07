public readonly struct LocationValueEvaluation
{
    public readonly float population01;
    public readonly string areaName;
    public readonly LocationValueModifiers modifiers;

    public LocationValueEvaluation(
        float population01,
        string areaName,
        LocationValueModifiers modifiers
    )
    {
        this.population01 = population01;
        this.areaName = areaName;
        this.modifiers = modifiers;
    }
}
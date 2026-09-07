public readonly struct LocationValueModifiers
{
    public readonly float constructionCostMultiplier;
    public readonly float maintenanceCostMultiplier;
    public readonly float commercialSalePriceMultiplier;
    public readonly float resourceProductionSpeedMultiplier;

    public LocationValueModifiers(
        float constructionCostMultiplier,
        float maintenanceCostMultiplier,
        float commercialSalePriceMultiplier,
        float resourceProductionSpeedMultiplier
    )
    {
        this.constructionCostMultiplier = constructionCostMultiplier;
        this.maintenanceCostMultiplier = maintenanceCostMultiplier;
        this.commercialSalePriceMultiplier = commercialSalePriceMultiplier;
        this.resourceProductionSpeedMultiplier = resourceProductionSpeedMultiplier;
    }

    public static LocationValueModifiers Default =>
        new LocationValueModifiers(1f, 1f, 1f, 1f);
}
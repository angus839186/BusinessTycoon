
public sealed partial class ProductionSystem
{
    private float GetConstructionCostMultiplier(double longitude, double latitude)
    {
        float multiplier = gameRules.ConstructionCostMultiplier;
        LocationValueModifiers modifiers = locationEconomyEvaluator.Evaluate(longitude, latitude);
        return multiplier * modifiers.constructionCostMultiplier;
    }
    public LocationValueEvaluation GetEconomyEvaluation(BuildingInstance building)
    {
        if (building == null)
            return new LocationValueEvaluation(0f, "None", LocationValueModifiers.Default);

        return locationEconomyEvaluator.EvaluateDetailed(building.Longitude, building.Latitude);
    }

    private LocationValueModifiers GetLocationModifiers(BuildingInstance building)
    {
        if (building == null)
            return LocationValueModifiers.Default;

        return locationEconomyEvaluator.Evaluate(
            building.Longitude,
            building.Latitude
        );
    }
}
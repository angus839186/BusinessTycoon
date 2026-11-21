using UnityEngine;

public sealed partial class ProductionSystem
{
    private float GetConstructionCostMultiplier(Vector3 worldPosition, double longitude, double latitude)
    {
        float multiplier = gameRules.ConstructionCostMultiplier;
        LocationValueModifiers modifiers = locationEconomyEvaluator.Evaluate(worldPosition, longitude, latitude);
        return multiplier * modifiers.constructionCostMultiplier;
    }
    public LocationValueEvaluation GetEconomyEvaluation(BuildingInstance building)
    {
        if (building == null)
            return new LocationValueEvaluation(0f, "None", LocationValueModifiers.Default);

        return locationEconomyEvaluator.EvaluateDetailed(building.WorldPosition, building.Longitude, building.Latitude);
    }
    public LocationValueModifiers GetEconomyModifiers(BuildingInstance building)
    {
        return GetLocationModifiers(building);
    }

    private LocationValueModifiers GetLocationModifiers(BuildingInstance building)
    {
        if (building == null)
            return LocationValueModifiers.Default;

        return locationEconomyEvaluator.Evaluate(
            building.WorldPosition,
            building.Longitude,
            building.Latitude
        );
    }
}
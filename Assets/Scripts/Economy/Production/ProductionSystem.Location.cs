using UnityEngine;

public sealed partial class ProductionSystem
{
    private float GetConstructionCostMultiplier(
    BuildingDefinition definition,
    Vector3 worldPosition,
    double longitude,
    double latitude
)
    {
        float multiplier = gameRules != null
            ? gameRules.ConstructionCostMultiplier
            : 1f;

        if (locationEconomyEvaluator != null)
        {
            LocationValueModifiers modifiers =
                locationEconomyEvaluator.Evaluate(worldPosition, longitude, latitude);

            multiplier *= modifiers.constructionCostMultiplier;
        }

        return multiplier;
    }
    public LocationValueEvaluation GetEconomyEvaluation(BuildingInstance building)
    {
        if (building == null || locationEconomyEvaluator == null)
        {
            return new LocationValueEvaluation(
                0f,
                "None",
                LocationValueModifiers.Default
            );
        }

        return locationEconomyEvaluator.EvaluateDetailed(
            building.WorldPosition,
            building.Longitude,
            building.Latitude
        );
    }
    public LocationValueModifiers GetEconomyModifiers(BuildingInstance building)
    {
        return GetLocationModifiers(building);
    }

    private LocationValueModifiers GetLocationModifiers(BuildingInstance building)
    {
        if (building == null || locationEconomyEvaluator == null)
            return LocationValueModifiers.Default;

        return locationEconomyEvaluator.Evaluate(
            building.WorldPosition,
            building.Longitude,
            building.Latitude
        );
    }
}
using UnityEngine;

public sealed class LocationValueEvaluator : MonoBehaviour
{
    [SerializeField] private bool useLocationModifiers = true;

    [Header("Multiplier Range")]
    [SerializeField] private float lowPopulationCostMultiplier = 0.5f;
    [SerializeField] private float highPopulationCostMultiplier = 2f;
    [SerializeField] private float lowPopulationCommercialPriceMultiplier = 0.7f;
    [SerializeField] private float highPopulationCommercialPriceMultiplier = 2f;
    [SerializeField] private float lowPopulationResourceSpeedMultiplier = 2f;
    [SerializeField] private float highPopulationResourceSpeedMultiplier = 0.8f;

    [SerializeField] private LocationValueArea[] areas;
    [SerializeField] private float fallbackPopulation01 = 0.2f;

    public LocationValueModifiers Evaluate(
        Vector3 worldPosition,
        double longitude,
        double latitude
    )
    {
        if (!useLocationModifiers)
            return LocationValueModifiers.Default;

        string areaName;
        float population01 = CalculatePopulation01(longitude, latitude, out areaName);

        float costMultiplier = Mathf.Lerp(
            lowPopulationCostMultiplier,
            highPopulationCostMultiplier,
            population01
        );

        float commercialPriceMultiplier = Mathf.Lerp(
            lowPopulationCommercialPriceMultiplier,
            highPopulationCommercialPriceMultiplier,
            population01
        );

        float resourceSpeedMultiplier = Mathf.Lerp(
            lowPopulationResourceSpeedMultiplier,
            highPopulationResourceSpeedMultiplier,
            population01
        );

        return new LocationValueModifiers(
            costMultiplier,
            costMultiplier,
            commercialPriceMultiplier,
            resourceSpeedMultiplier
        );
    }

    private float CalculatePopulation01(
    double longitude,
    double latitude,
    out string areaName
)
    {
        areaName = "Fallback";

        if (areas == null || areas.Length == 0)
            return Mathf.Clamp01(fallbackPopulation01);

        float highestPopulation = 0f;

        foreach (LocationValueArea area in areas)
        {
            if (area == null || area.radiusDegrees <= 0)
                continue;

            double dx = longitude - area.longitude;
            double dy = latitude - area.latitude;
            double distance = System.Math.Sqrt(dx * dx + dy * dy);

            float influence01 = Mathf.Clamp01(
                1f - (float)(distance / area.radiusDegrees)
            );

            float weightedPopulation = Mathf.Clamp01(area.population01) * influence01;

            if (weightedPopulation > highestPopulation)
            {
                highestPopulation = weightedPopulation;
                areaName = string.IsNullOrWhiteSpace(area.displayName)
                    ? area.areaId
                    : area.displayName;
            }
        }

        float fallback = Mathf.Clamp01(fallbackPopulation01);

        if (highestPopulation < fallback)
            areaName = "Fallback";

        return Mathf.Max(highestPopulation, fallback);
    }
    public LocationValueEvaluation EvaluateDetailed(
    Vector3 worldPosition,
    double longitude,
    double latitude
)
    {
        string areaName;
        float population01 = CalculatePopulation01(longitude, latitude, out areaName);
        LocationValueModifiers modifiers = Evaluate(worldPosition, longitude, latitude);

        return new LocationValueEvaluation(
            population01,
            areaName,
            modifiers
        );
    }
}
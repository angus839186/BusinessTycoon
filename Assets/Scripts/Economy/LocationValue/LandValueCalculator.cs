using System;
using UnityEngine;

public static class LandValueCalculator
{
    public static float CalculateScore(
        double density,
        LandValueConfig config
    )
    {
        if (config == null)
            return 0f;

        return CalculateScore(
            density,
            config.maxScore,
            config.densityExponent,
            config.midpointDensity
        );
    }

    public static float Calculate01(
        double density,
        LandValueConfig config
    )
    {
        if (config == null || config.maxScore <= 0f)
            return 0f;

        float score = CalculateScore(density, config);

        return Mathf.Clamp01(
            score / config.maxScore
        );
    }

    private static float CalculateScore(
        double density,
        double maxScore,
        double exponent,
        double midpointDensity
    )
    {
        if (
            double.IsNaN(density) ||
            density <= 0d ||
            maxScore <= 0d ||
            exponent <= 0d ||
            midpointDensity <= 0d
        )
        {
            return 0f;
        }

        if (double.IsPositiveInfinity(density))
            return (float)maxScore;

        double ratio = midpointDensity / density;
        double power = Math.Pow(ratio, exponent);
        double score = maxScore / (1d + power);

        if (double.IsNaN(score))
            return 0f;

        score = Math.Max(
            0d,
            Math.Min(maxScore, score)
        );

        return (float)score;
    }
}
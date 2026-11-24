using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class ProductionSystem
{
    public bool TryPayConstructionCost(
    BuildingDefinition definition,
    double longitude,
    double latitude,
    out ResourceAmount[] paidCosts
)
    {
        paidCosts = Array.Empty<ResourceAmount>();

        if (definition == null)
            return false;

        float multiplier = GetConstructionCostMultiplier(longitude, latitude);

        paidCosts = CalculateModifiedCosts(
            definition.constructionCosts,
            multiplier
        );

        return TryPay(
            paidCosts,
            definition.displayName,
            "Construction"
        );
    }
    public bool TryPayDemolitionCost(BuildingInstance building, float demolitionCostRate)
    {
        if (building == null)
            return false;

        List<ResourceAmount> demolitionCosts = new List<ResourceAmount>();

        foreach (ResourceAmount cost in building.ConstructionCostSnapshot)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            int amount = Mathf.CeilToInt(cost.amount * demolitionCostRate);

            if (amount <= 0)
                continue;

            demolitionCosts.Add(new ResourceAmount
            {
                resource = cost.resource,
                amount = amount
            });
        }

        return TryPay(
            demolitionCosts.ToArray(),
            building.Definition != null ? building.Definition.displayName : building.InstanceId,
            "Demolition"
        );
    }


    private ResourceAmount[] CalculateModifiedCosts(ResourceAmount[] baseCosts, float multiplier)
    {
        if (baseCosts == null || baseCosts.Length == 0)
            return Array.Empty<ResourceAmount>();

        List<ResourceAmount> result = new List<ResourceAmount>();

        foreach (ResourceAmount cost in baseCosts)
        {
            if (cost == null || cost.resource == null || cost.amount <= 0)
                continue;

            result.Add(new ResourceAmount
            {
                resource = cost.resource,
                amount = Mathf.CeilToInt(cost.amount * multiplier)
            });
        }

        return result.ToArray();
    }


}
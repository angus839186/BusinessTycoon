using System;
using UnityEngine;

public sealed class MarketSystem : MonoBehaviour
{
    public event Action PricesChanged;

    public int GetUnitPrice(ResourceDefinition resource)
    {
        if (resource == null) return 0;

        return Mathf.Max(
            0,
            resource.basePrice
        );
    }

    private void MarkPricesChanged()
    {
        PricesChanged?.Invoke();
    }
}
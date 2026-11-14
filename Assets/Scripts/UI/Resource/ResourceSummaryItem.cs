using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResourceSummaryItem : MonoBehaviour
{
    private MarketSystem marketSystem;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI basePriceText;
    [SerializeField] private TextMeshProUGUI totalValueText;

    private ResourceDefinition definition;

    public void Initialize(ResourceDefinition resource, MarketSystem market)
    {
        definition = resource;
        marketSystem = market;

        if (definition == null)
            return;

        if (nameText != null)
        {
            nameText.text =
                string.IsNullOrWhiteSpace(definition.displayName)
                    ? definition.resourceId
                    : definition.displayName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = definition.icon;
            iconImage.enabled = definition.icon != null;
        }

        if (basePriceText != null)
        {
            basePriceText.text =
                GetUnitPrice().ToString();
        }

        SetAmount(0);
    }

    public void SetAmount(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (amountText != null)
            amountText.text = amount.ToString();

        int unitPrice = GetUnitPrice();

        if (basePriceText != null)
        {
            basePriceText.text =
                unitPrice.ToString();
        }

        if (totalValueText != null)
        {
            long totalValue =
                (long)unitPrice * amount;

            totalValueText.text =
                totalValue.ToString();
        }
    }

    private int GetUnitPrice()
    {
        if (definition == null)
            return 0;

        return marketSystem != null
            ? marketSystem.GetUnitPrice(definition)
            : Mathf.Max(0, definition.basePrice);
    }
}
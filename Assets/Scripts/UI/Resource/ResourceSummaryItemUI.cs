using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResourceSummaryItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI basePriceText;
    [SerializeField] private TextMeshProUGUI totalValueText;

    private ResourceDefinition definition;

    public void Initialize(ResourceDefinition resource)
    {
        definition = resource;

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
            basePriceText.text = definition.basePrice.ToString();

        SetAmount(0);
    }

    public void SetAmount(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (amountText != null)
            amountText.text = amount.ToString();

        if (totalValueText != null)
        {
            long totalValue =
                (long)Mathf.Max(0, definition.basePrice) * amount;

            totalValueText.text = totalValue.ToString();
        }
    }
}
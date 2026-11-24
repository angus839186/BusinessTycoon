using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BuildingCatalogItem : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject selectedIndicator;

    private BuildingDefinition definition;
    private Action<BuildingDefinition> selectedCallback;

    private void Awake()
    {
        if (selectButton == null)
        {
            selectButton = GetComponent<Button>();
        }

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Initialize(
        BuildingDefinition building,
        Action<BuildingDefinition> onSelected
    )
    {
        definition = building;
        selectedCallback = onSelected;

        if (nameText != null)
        {
            nameText.text =
                definition != null &&
                !string.IsNullOrWhiteSpace(definition.displayName)
                    ? definition.displayName
                    : definition != null
                        ? definition.buildingId
                        : "";
        }

        if (categoryText != null)
        {
            categoryText.text =
                definition != null
                    ? FormatBuildingSummary(definition)
                    : "";
        }

        if (costText != null)
        {
            costText.text =
                definition != null
                    ? $"基礎成本：{FormatResourceAmounts(definition.constructionCosts)}"
                    : "";
        }

        if (iconImage != null)
        {
            iconImage.sprite =
                definition != null
                    ? definition.icon
                    : null;

            iconImage.enabled =
                iconImage.sprite != null;
        }

        if (selectButton != null)
        {
            selectButton.interactable =
                definition != null &&
                definition.isEnabled;
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }

    private void HandleClicked()
    {
        if (definition == null ||
            !definition.isEnabled)
        {
            return;
        }

        selectedCallback?.Invoke(definition);
    }


}
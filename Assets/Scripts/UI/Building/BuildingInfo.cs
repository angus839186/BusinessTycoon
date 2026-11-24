using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BuildingInfo : MonoBehaviour
{
    [SerializeField] private BuildingSelector selector;

    [Header("Header")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private Button closeButton;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Production")]
    [SerializeField] private GameObject progressRoot;
    [SerializeField] private TextMeshProUGUI productionText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Inventory")]
    [SerializeField] private TextMeshProUGUI inventoryText;

    [Header("Location")]
    [SerializeField] private TextMeshProUGUI locationText;

    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private GameClock gameClock;
    [SerializeField] private PanelController panelController;
    [SerializeField] private int maxInventoryLines = 3;

    [Header("Display")]
    [SerializeField] private UiTransition transition;
    private BuildingInstance selectedBuilding;
    private bool panelOpen;
    private bool HasRequiredReferences =>
    selector != null &&
    iconImage != null &&
    nameText != null &&
    categoryText != null &&
    closeButton != null &&
    statusText != null &&
    progressRoot != null &&
    productionText != null &&
    progressSlider != null &&
    progressText != null &&
    inventoryText != null &&
    locationText != null &&
    productionSystem != null &&
    gameClock != null &&
    panelController != null &&
    transition != null;

    private void Awake()
    {
        if (selector == null)
        {
            selector = FindFirstObjectByType<BuildingSelector>();
        }

        if (productionSystem == null)
        {
            productionSystem = FindFirstObjectByType<ProductionSystem>();
        }

        if (gameClock == null)
        {
            gameClock = FindFirstObjectByType<GameClock>();
        }

        if (panelController == null)
        {
            panelController = FindFirstObjectByType<PanelController>();
        }

        if (!HasRequiredReferences)
        {
            Debug.LogError(
                "BuildingInfo 缺少必要的系統或 UI 參考。",
                this
            );

            enabled = false;
            return;
        }

        transition.Hide(true);
    }

}
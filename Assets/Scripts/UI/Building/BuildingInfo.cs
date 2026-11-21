using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuildingInfo : MonoBehaviour
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

    private readonly StringBuilder builder = new StringBuilder();
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

    private void OnEnable()
    {
        selector.SelectionChanged += HandleSelectionChanged;

        productionSystem.DataChanged += Refresh;
        gameClock.TimeChanged += Refresh;
        panelController.PanelStateChanged += HandlePanelStateChanged;
        panelOpen = panelController.HasOpenPanel;
        closeButton.onClick.AddListener(HandleCloseClicked);

        HandleSelectionChanged(selector.SelectedBuilding);
    }

    private void OnDisable()
    {
        if (selector != null)
        {
            selector.SelectionChanged -= HandleSelectionChanged;
        }

        if (productionSystem != null)
        {
            productionSystem.DataChanged -= Refresh;
        }

        if (gameClock != null)
        {
            gameClock.TimeChanged -= Refresh;
        }

        if (panelController != null)
        {
            panelController.PanelStateChanged -= HandlePanelStateChanged;
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        UnsubscribeBuilding();
        selectedBuilding = null;
        if (transition != null)
        {
            transition.Hide(true);
        }
    }
    private void HandleCloseClicked()
    {
        selector.ClearSelection();
    }

    private void HandleSelectionChanged(BuildingInstance building)
    {
        UnsubscribeBuilding();
        selectedBuilding = building;

        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged += HandleOperationalChanged;
        }

        Refresh();
    }

    private void HandleOperationalChanged(bool operational
    )
    {
        Refresh();
    }

    private void HandlePanelStateChanged(bool isOpen)
    {
        panelOpen = isOpen;
        Refresh();
    }

    private void UnsubscribeBuilding()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged -= HandleOperationalChanged;
        }
    }

    private void Refresh()
    {
        if (selectedBuilding == null || panelOpen)
        {
            transition.Hide();
            return;
        }

        BuildingDefinition definition = selectedBuilding.Definition;

        if (definition == null)
        {
            transition.Hide();
            return;
        }

        nameText.text = definition.displayName;

        categoryText.text =
            GetCategoryText(
                definition.category
            );

        statusText.text = selectedBuilding.IsOperational ? "運作中" : "已停用";

        iconImage.sprite = definition.icon;

        iconImage.enabled = definition.icon != null;

        RefreshProduction(definition);
        RefreshInventory();
        RefreshLocation();

        transition.Show();
    }

    private string GetCategoryText(
    BuildingCategory category
)
    {
        return category switch
        {
            BuildingCategory.Resource => "資源建築",
            BuildingCategory.Processing => "加工建築",
            BuildingCategory.Commercial => "商業建築",
            BuildingCategory.Storage => "倉庫",
            _ => "其他"
        };
    }

    private void RefreshProduction(
    BuildingDefinition definition)
    {
        if (definition.category == BuildingCategory.Storage
        )
        {
            productionText.text =
                "此建築不進行生產";

            progressRoot.SetActive(false);
            return;
        }

        progressRoot.SetActive(true);

        if (definition.category == BuildingCategory.Commercial)
        {
            productionText.text =
                definition.saleResource != null
                    ? $"販售：{definition.saleResource.displayName} × {definition.saleAmount}"
                    : "尚未設定販售商品";
        }
        else
        {
            productionText.text =
                definition.producedResource != null
                    ? $"產出：{definition.producedResource.displayName}"
                    : "尚未設定產出資源";
        }

        float progress = 0f;
        string progressLabel;

        if (!selectedBuilding.IsOperational)
        {
            progressLabel = "已停止";
        }
        else if (
            productionSystem.IsWaitingForInput(selectedBuilding)
        )
        {
            progressLabel = "等待輸入資源";
        }
        else
        {
            progress = productionSystem.GetProcessProgress(selectedBuilding);

            progressLabel = $"{progress:P0}";
        }

        progressSlider.value = progress;
        progressText.text = progressLabel;
    }

    private void RefreshInventory()
    {
        builder.Clear();
        int shown = 0;

        foreach (ResourceStack stack in selectedBuilding.Inventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
            {
                continue;
            }

            if (shown > 0)
            {
                builder.AppendLine();
            }

            builder.Append($"{stack.resource.displayName} × {stack.amount}");

            shown++;

            if (shown >= maxInventoryLines)
            {
                break;
            }
        }

        inventoryText.text = shown > 0 ? builder.ToString() : "目前沒有庫存";
    }

    private void RefreshLocation()
    {
        LocationValueEvaluation evaluation =
            productionSystem.GetEconomyEvaluation(selectedBuilding);

        bool hasArea = !string.IsNullOrWhiteSpace(evaluation.areaName) &&
        evaluation.areaName != "None";

        locationText.text = hasArea ? $"地區：{evaluation.areaName}" : "地區：尚無資料";
    }
}
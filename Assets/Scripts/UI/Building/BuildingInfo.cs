using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public sealed class BuildingInfo : MonoBehaviour
{
    [SerializeField] private BuildingSelector selector;

    [SerializeField] private RectTransform infoRoot;

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

    [Header("Animation")]
    [SerializeField] private CanvasGroup infoCanvasGroup;
    [SerializeField, Min(0.05f)]
    private float tweenDuration = 0.2f;
    [SerializeField]
    private Vector2 hiddenOffset = new Vector2(48f, 0f);

    private Vector2 shownPosition;
    private Sequence infoSequence;
    private bool isVisible;

    private readonly StringBuilder builder =
        new StringBuilder();

    private BuildingInstance selectedBuilding;
    private bool panelOpen;

    private void Awake()
    {
        if (selector == null)
            selector =
                FindFirstObjectByType<BuildingSelector>();

        if (productionSystem == null)
            productionSystem =
                FindFirstObjectByType<ProductionSystem>();

        if (gameClock == null)
            gameClock =
                FindFirstObjectByType<GameClock>();

        if (panelController == null)
            panelController =
                FindFirstObjectByType<PanelController>();

        if (
    infoCanvasGroup == null &&
    infoRoot != null
)
        {
            infoCanvasGroup =
                infoRoot.GetComponent<CanvasGroup>();
        }

        if (infoRoot != null)
        {
            shownPosition = infoRoot.anchoredPosition;
        }

        Hide(true);
    }

    private void OnEnable()
    {
        if (selector != null)
        {
            selector.SelectionChanged += HandleSelectionChanged;
        }

        if (productionSystem != null)
        {
            productionSystem.DataChanged += Refresh;
        }

        if (gameClock != null)
        {
            gameClock.TimeChanged += Refresh;
        }

        if (panelController != null)
        {
            panelController.PanelStateChanged += HandlePanelStateChanged;
            panelOpen = panelController.HasOpenPanel;
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        HandleSelectionChanged(
            selector != null
                ? selector.SelectedBuilding
                : null
        );
    }

    private void OnDisable()
    {
        if (selector != null)
        {
            selector.SelectionChanged -=
                HandleSelectionChanged;
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
            panelController.PanelStateChanged -=
                HandlePanelStateChanged;
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        UnsubscribeBuilding();
        selectedBuilding = null;
        Hide(true);
    }
    private void OnDestroy()
    {
        infoSequence?.Kill();
    }
    private void HandleCloseClicked()
    {
        if (selector != null)
        {
            selector.ClearSelection();
        }
    }

    private void HandleSelectionChanged(
        BuildingInstance building
    )
    {
        UnsubscribeBuilding();

        selectedBuilding = building;

        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged +=
                HandleOperationalChanged;
        }

        Refresh();
    }

    private void HandleOperationalChanged(
        bool operational
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
            selectedBuilding.OperationalStateChanged -=
                HandleOperationalChanged;
        }
    }

    private void Refresh()
    {
        if (
            selectedBuilding == null ||
            panelOpen ||
            infoRoot == null
        )
        {
            Hide();
            return;
        }

        BuildingDefinition definition =
            selectedBuilding.Definition;

        if (definition == null)
        {
            Hide();
            return;
        }

        if (nameText != null)
        {
            nameText.text = definition.displayName;
        }

        if (categoryText != null)
        {
            categoryText.text =
                GetCategoryText(definition.category);
        }

        if (statusText != null)
        {
            statusText.text =
                selectedBuilding.IsOperational
                    ? "運作中"
                    : "已停用";
        }

        if (iconImage != null)
        {
            iconImage.sprite = definition.icon;
            iconImage.enabled = definition.icon != null;
        }

        RefreshProduction(definition);
        RefreshInventory();
        RefreshLocation();

        Show();
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
        BuildingDefinition definition
    )
    {
        if (definition.category == BuildingCategory.Storage)
        {
            if (productionText != null)
            {
                productionText.text = "此建築不進行生產";
            }

            if (progressRoot != null)
            {
                progressRoot.SetActive(false);
            }

            return;
        }

        if (progressRoot != null)
        {
            progressRoot.SetActive(true);
        }

        if (productionText != null)
        {
            if (
                definition.category ==
                BuildingCategory.Commercial
            )
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
        }

        float progress = 0f;
        string progressLabel;

        if (!selectedBuilding.IsOperational)
        {
            progressLabel = "已停止";
        }
        else if (productionSystem == null)
        {
            progressLabel = "無法取得生產資料";
        }
        else if (
            productionSystem.IsWaitingForInput(
                selectedBuilding
            )
        )
        {
            progressLabel = "等待輸入資源";
        }
        else
        {
            progress =
                productionSystem.GetProcessProgress(
                    selectedBuilding
                );

            progressLabel = $"{progress:P0}";
        }

        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }

        if (progressText != null)
        {
            progressText.text = progressLabel;
        }
    }

    private void RefreshInventory()
    {
        if (inventoryText == null)
        {
            return;
        }

        builder.Clear();

        int shown = 0;

        foreach (
            ResourceStack stack
            in selectedBuilding.Inventory.Resources
        )
        {
            if (
                stack == null ||
                stack.resource == null ||
                stack.amount <= 0
            )
            {
                continue;
            }

            if (shown > 0)
            {
                builder.AppendLine();
            }

            builder.Append(
                $"{stack.resource.displayName} × {stack.amount}"
            );

            shown++;

            if (shown >= maxInventoryLines)
            {
                break;
            }
        }

        inventoryText.text =
            shown > 0
                ? builder.ToString()
                : "目前沒有庫存";
    }

    private void RefreshLocation()
    {
        if (locationText == null)
        {
            return;
        }

        if (productionSystem == null)
        {
            locationText.text = "地區：尚無資料";
            return;
        }

        LocationValueEvaluation evaluation =
            productionSystem.GetEconomyEvaluation(
                selectedBuilding
            );

        bool hasArea =
            !string.IsNullOrWhiteSpace(evaluation.areaName) &&
            evaluation.areaName != "None";

        locationText.text =
            hasArea
                ? $"地區：{evaluation.areaName}"
                : "地區：尚無資料";
    }
    private void Show()
    {
        if (infoRoot == null)
        {
            return;
        }

        if (
            isVisible &&
            infoRoot.gameObject.activeSelf
        )
        {
            return;
        }

        isVisible = true;

        infoSequence?.Kill();
        infoSequence = null;

        Vector2 hiddenPosition =
            shownPosition + hiddenOffset;

        if (!infoRoot.gameObject.activeSelf)
        {
            infoRoot.gameObject.SetActive(true);
            infoRoot.anchoredPosition = hiddenPosition;

            if (infoCanvasGroup != null)
            {
                infoCanvasGroup.alpha = 0f;
            }
        }

        if (infoCanvasGroup == null)
        {
            infoRoot.anchoredPosition = shownPosition;
            return;
        }

        infoCanvasGroup.interactable = false;
        infoCanvasGroup.blocksRaycasts = false;

        infoSequence = DOTween.Sequence()
            .SetUpdate(true)
            .Append(
                infoRoot
                    .DOAnchorPos(
                        shownPosition,
                        tweenDuration
                    )
                    .SetEase(Ease.OutCubic)
            )
            .Join(
                infoCanvasGroup.DOFade(
                    1f,
                    tweenDuration
                )
            )
            .OnComplete(() =>
            {
                if (!isVisible)
                {
                    return;
                }

                infoCanvasGroup.interactable = true;
                infoCanvasGroup.blocksRaycasts = true;
                infoSequence = null;
            });
    }

    private void Hide(bool instant = false)
    {
        if (infoRoot == null)
        {
            return;
        }

        isVisible = false;

        infoSequence?.Kill();
        infoSequence = null;

        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.interactable = false;
            infoCanvasGroup.blocksRaycasts = false;
        }

        Vector2 hiddenPosition =
            shownPosition + hiddenOffset;

        if (
            instant ||
            !infoRoot.gameObject.activeSelf ||
            infoCanvasGroup == null
        )
        {
            infoRoot.anchoredPosition = hiddenPosition;

            if (infoCanvasGroup != null)
            {
                infoCanvasGroup.alpha = 0f;
            }

            infoRoot.gameObject.SetActive(false);
            return;
        }

        infoSequence = DOTween.Sequence()
            .SetUpdate(true)
            .Append(
                infoRoot
                    .DOAnchorPos(
                        hiddenPosition,
                        tweenDuration
                    )
                    .SetEase(Ease.InCubic)
            )
            .Join(
                infoCanvasGroup.DOFade(
                    0f,
                    tweenDuration
                )
            )
            .OnComplete(() =>
            {
                if (isVisible)
                {
                    return;
                }

                infoRoot.gameObject.SetActive(false);
                infoSequence = null;
            });
    }
}
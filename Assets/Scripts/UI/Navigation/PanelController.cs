using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using DG.Tweening;

public sealed class PanelController : MonoBehaviour
{
    [SerializeField] private Button menuToggleButton;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private Transform panelRoot;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private bool openFirstPanelOnStart;
    [SerializeField] private GameInput gameInput;

    [Header("Menu Animation")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField, Min(0.05f)] private float menuTweenDuration = 0.2f;
    [SerializeField] private Vector2 menuHiddenOffset = new Vector2(-180f, 0f);

    private RectTransform menuRect;
    private Vector2 menuOpenPosition;
    private Sequence menuSequence;
    private bool isMenuOpen;
    public bool HasOpenPanel => currentPanel != null;
    public event Action<bool> PanelStateChanged;
    private readonly Dictionary<GameObject, GameObject> panelInstances =
    new Dictionary<GameObject, GameObject>();
    private GameObject currentPanel;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }
        menuRect = buttonRoot as RectTransform;

        if (menuCanvasGroup == null && buttonRoot != null)
        {
            menuCanvasGroup = buttonRoot.GetComponent<CanvasGroup>();
        }

        if (menuRect != null)
        {
            menuOpenPosition = menuRect.anchoredPosition;
        }

        SetMenuOpen(false, true);
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed += HandleCancelPressed;
            gameInput.MenuTogglePressed += HandleMenuTogglePressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.CancelPressed -= HandleCancelPressed;
            gameInput.MenuTogglePressed -= HandleMenuTogglePressed;
        }
    }

    private void Start()
    {
        CreateButtons();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCurrentPanel);
        }

        if (menuToggleButton != null)
        {
            menuToggleButton.onClick.AddListener(ToggleMenu);
        }

        CloseCurrentPanel();

        if (
            openFirstPanelOnStart &&
            panels != null &&
            panels.Length > 0 &&
            panels[0] != null
        )
        {
            OpenPanel(panels[0]);
        }
    }

    private void HandleCancelPressed()
    {
        if (currentPanel != null)
        {
            CloseCurrentPanel();
            return;
        }

        if (isMenuOpen)
        {
            SetMenuOpen(false);
        }
    }


    private void OnDestroy()
    {
        menuSequence?.Kill();
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseCurrentPanel);
        if (menuToggleButton != null)
            menuToggleButton.onClick.RemoveListener(ToggleMenu);
    }

    private void HandleMenuTogglePressed()
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (currentPanel != null)
        {
            CloseCurrentPanel();
            SetMenuOpen(true);
            return;
        }

        SetMenuOpen(!isMenuOpen);
    }

    private void SetMenuOpen(bool open, bool instant = false)
    {
        isMenuOpen = open;
        if (buttonRoot == null) return;

        menuSequence?.Kill();
        menuSequence = null;

        Vector2 closedPosition = menuOpenPosition + menuHiddenOffset;

        if (instant || menuRect == null || menuCanvasGroup == null)
        {
            if (menuRect != null)
            {
                menuRect.anchoredPosition = open ? menuOpenPosition : closedPosition;
            }

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = open ? 1f : 0f;
                menuCanvasGroup.interactable = open;
                menuCanvasGroup.blocksRaycasts = open;
            }

            buttonRoot.gameObject.SetActive(open);
            return;
        }

        bool wasActive = buttonRoot.gameObject.activeSelf;

        if (open)
        {
            buttonRoot.gameObject.SetActive(true);

            if (!wasActive)
            {
                menuRect.anchoredPosition = closedPosition;
                menuCanvasGroup.alpha = 0f;
            }
        }
        else if (!wasActive)
        {
            menuRect.anchoredPosition = closedPosition;
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            return;
        }

        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        Vector2 targetPosition = open ? menuOpenPosition : closedPosition;

        Ease ease = open ? Ease.OutCubic : Ease.InCubic;

        menuSequence = DOTween.Sequence()
            .SetUpdate(true)
            .Append(
            menuRect.DOAnchorPos(targetPosition, menuTweenDuration)
            .SetEase(ease)
            )
            .Join(
                menuCanvasGroup.DOFade(open ? 1f : 0f, menuTweenDuration)
            )
            .OnComplete(() =>
            {
                menuCanvasGroup.interactable = open;
                menuCanvasGroup.blocksRaycasts = open;

                if (!open) buttonRoot.gameObject.SetActive(false);

                menuSequence = null;
            });
    }

    private void CreateButtons()
    {
        if (buttonPrefab == null || buttonRoot == null || panels == null)
            return;

        foreach (GameObject panel in panels)
        {
            if (panel == null) continue;

            GameObject targetPanel = panel;
            Button button = Instantiate(buttonPrefab, buttonRoot);
            PanelButton buttonUI = button.GetComponent<PanelButton>();

            if (buttonUI != null)
            {
                buttonUI.SetContent(targetPanel);
            }
            else
            {
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null) label.text = targetPanel.name;
            }

            button.onClick.AddListener(() => OpenPanel(targetPanel));
            button.gameObject.SetActive(true);
        }
    }

    public void OpenPanel(GameObject panelPrefab)
    {
        if (panelPrefab == null) return;
        if (panelRoot == null)
        {
            Debug.LogError("PanelController 尚未設定 Panel Root。", this);
            return;
        }

        bool wasOpen = currentPanel != null;

        SetAllPanelsInactive();

        if (!panelInstances.TryGetValue(panelPrefab, out GameObject panelInstance) || panelInstance == null)
        {
            panelInstance = Instantiate(panelPrefab, panelRoot, false);
            panelInstances[panelPrefab] = panelInstance;
        }

        currentPanel = panelInstance;
        currentPanel.SetActive(true);

        if (gameInput != null) gameInput.SetMapEnabled(false);

        if (!wasOpen) PanelStateChanged?.Invoke(true);

        SetMenuOpen(false);

        if (closeButton != null) closeButton.gameObject.SetActive(true);
    }

    public void CloseCurrentPanel()
    {
        bool wasOpen = currentPanel != null;
        SetAllPanelsInactive();
        currentPanel = null;
        if (gameInput != null) gameInput.SetMapEnabled(true);

        if (wasOpen) PanelStateChanged?.Invoke(false);

        if (closeButton != null) closeButton.gameObject.SetActive(false);

        SetMenuOpen(false);
    }

    private void SetAllPanelsInactive()
    {
        foreach (GameObject panelInstance in panelInstances.Values)
        {
            if (panelInstance != null) panelInstance.SetActive(false);
        }
    }
}
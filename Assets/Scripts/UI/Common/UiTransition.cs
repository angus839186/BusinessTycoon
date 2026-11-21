using DG.Tweening;
using UnityEngine;

public sealed class UiTransition : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform root;

    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField, Min(0.05f)] private float duration = 0.2f;

    [SerializeField]
    private Vector2 hiddenOffset = new Vector2(48f, 0f);

    private Vector2 shownPosition;
    private Sequence sequence;
    private bool initialized;

    public bool IsVisible
    {
        get;
        private set;
    }

    private void Awake()
    {
        TryInitialize();
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    public void Show(bool instant = false)
    {
        SetVisible(true, instant);
    }

    public void Hide(bool instant = false)
    {
        SetVisible(false, instant);
    }

    public void SetVisible(bool visible, bool instant = false)
    {
        if (!TryInitialize())
        {
            return;
        }

        IsVisible = visible;

        sequence?.Kill();
        sequence = null;

        bool wasActive = root.gameObject.activeSelf;

        Vector2 hiddenPosition = shownPosition + hiddenOffset;

        Vector2 targetPosition =
            visible ? shownPosition : hiddenPosition;

        if (visible && !wasActive)
        {
            root.gameObject.SetActive(true);
            root.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0f;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (instant || (!visible && !wasActive))
        {
            ApplyState(visible, targetPosition);

            return;
        }

        Ease ease = visible ? Ease.OutCubic : Ease.InCubic;

        sequence = DOTween.Sequence()
            .SetUpdate(true)
            .Append(root.DOAnchorPos(targetPosition, duration).SetEase(ease))
            .Join(canvasGroup.DOFade(visible ? 1f : 0f, duration))
            .OnComplete(() =>
            {
                canvasGroup.interactable = visible;

                canvasGroup.blocksRaycasts = visible;

                if (!visible)
                {
                    root.gameObject.SetActive(false);
                }

                sequence = null;
            });
    }

    private bool TryInitialize()
    {
        if (initialized)
        {
            return true;
        }

        if (root == null || canvasGroup == null)
        {
            Debug.LogError("UiTransition 缺少 Root 或 CanvasGroup。", this);

            enabled = false;
            return false;
        }

        shownPosition = root.anchoredPosition;

        initialized = true;
        return true;
    }

    private void ApplyState(bool visible, Vector2 position)
    {
        root.anchoredPosition = position;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        root.gameObject.SetActive(visible);
    }
}
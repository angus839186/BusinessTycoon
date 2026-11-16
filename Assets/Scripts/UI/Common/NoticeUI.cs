using System.Collections;
using TMPro;
using UnityEngine;

public sealed class NoticeUI : MonoBehaviour
{
    public enum NoticeStyle
    {
        Info,
        Success,
        Error
    }

    [Header("UI")]
    [SerializeField] private CanvasGroup root;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField] private float visibleSeconds = 2f;
    [SerializeField] private float fadeSeconds = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color infoColor = Color.white;
    [SerializeField]
    private Color successColor =
        new Color(0.4f, 1f, 0.5f);
    [SerializeField]
    private Color errorColor =
        new Color(1f, 0.35f, 0.35f);

    private Coroutine showCoroutine;

    private void Awake()
    {
        if (root == null)
            root = GetComponent<CanvasGroup>();

        HideImmediately();
    }

    private void OnDisable()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        HideImmediately();
    }

    public void ShowInfo(string message)
    {
        Show(message, NoticeStyle.Info);
    }

    public void ShowSuccess(string message)
    {
        Show(message, NoticeStyle.Success);
    }

    public void ShowError(string message)
    {
        Show(message, NoticeStyle.Error);
    }

    public void Show(
        string message,
        NoticeStyle style
    )
    {
        if (
            root == null ||
            messageText == null ||
            string.IsNullOrWhiteSpace(message)
        )
        {
            return;
        }

        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        messageText.text = message;
        messageText.color = GetColor(style);

        showCoroutine =
            StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return Fade(0f, 1f);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, visibleSeconds)
        );

        yield return Fade(1f, 0f);

        showCoroutine = null;
    }

    private IEnumerator Fade(
        float startAlpha,
        float endAlpha
    )
    {
        float duration =
            Mathf.Max(0f, fadeSeconds);

        if (duration <= 0f)
        {
            root.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;
        root.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            root.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                elapsed / duration
            );

            yield return null;
        }

        root.alpha = endAlpha;
    }

    private Color GetColor(NoticeStyle style)
    {
        return style switch
        {
            NoticeStyle.Success => successColor,
            NoticeStyle.Error => errorColor,
            _ => infoColor
        };
    }

    private void HideImmediately()
    {
        if (root == null)
            return;

        root.alpha = 0f;
        root.interactable = false;
        root.blocksRaycasts = false;
    }
}
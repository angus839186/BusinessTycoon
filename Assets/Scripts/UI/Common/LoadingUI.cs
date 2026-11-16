using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject screenRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Display")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    public bool IsVisible =>
        screenRoot != null &&
        screenRoot.activeSelf;

    private void Awake()
    {
        Hide();
    }

    public void Show(string message = "載入中...")
    {
        if (screenRoot == null)
        {
            Debug.LogError(
                "LoadingUI 尚未設定 Screen Root。",
                this
            );

            return;
        }

        screenRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        SetProgress(0f);
    }

    public void SetProgress(float progress)
    {
        float value = Mathf.Clamp01(progress);

        if (progressBar != null)
        {
            progressBar.value = value;
        }

        if (progressText != null)
        {
            progressText.text =
                $"{value * 100f:0}%";
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (screenRoot != null)
        {
            screenRoot.SetActive(false);
        }
    }
}
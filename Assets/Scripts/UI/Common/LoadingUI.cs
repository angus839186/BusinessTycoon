using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingUI : MonoBehaviour
{
    [SerializeField] private UiTransition transition;

    [Header("Display")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    public bool IsVisible => transition != null && transition.IsVisible;

    private bool HasRequiredReferences =>
        transition != null &&
        messageText != null &&
        progressBar != null &&
        progressText != null;

    private void Awake()
    {
        if (!HasRequiredReferences)
        {
            Debug.LogError("LoadingUI 缺少必要的 UI 參考。", this);
            enabled = false;
            return;
        }

        Hide();
    }

    public void Show(string message = "載入中...")
    {
        messageText.text = string.IsNullOrWhiteSpace(message) ? "載入中..." : message;
        SetProgress(0f);
        transition.Show(true);
    }

    public void SetProgress(float progress)
    {
        float value = Mathf.Clamp01(progress);
        progressBar.value = value;
        progressText.text = $"{value * 100f:0}%";
    }

    public void Hide()
    {
        if (transition != null)
        {
            transition.Hide(true);
        }
    }
}
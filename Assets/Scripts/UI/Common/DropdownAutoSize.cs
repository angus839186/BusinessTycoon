using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class DropdownAutoSize : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float minWidth = 140f;
    [SerializeField] private float padding = 48f;
    [SerializeField] private float height = 32f;

    private RectTransform rectTransform;
    private bool initialized;

    private bool HasRequiredReferences => rectTransform != null && dropdown != null && label != null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        if (label == null && dropdown != null)
            label = dropdown.captionText as TextMeshProUGUI;

        if (!HasRequiredReferences)
        {
            Debug.LogError("DropdownAutoSize 缺少必要參考。", this);
            enabled = false;
            return;
        }

        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        dropdown.onValueChanged.AddListener(HandleValueChanged);
        Resize();
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        dropdown.onValueChanged.RemoveListener(HandleValueChanged);
    }

    public void Resize()
    {
        if (!initialized)
            return;

        float width = Mathf.Max(minWidth, label.preferredWidth + padding);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void HandleValueChanged(int _) => Resize();
}
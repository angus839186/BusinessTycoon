using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class TmpDropdownAutoSize : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float minWidth = 140f;
    [SerializeField] private float padding = 48f;
    [SerializeField] private float height = 32f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        if (label == null && dropdown != null)
            label = (TextMeshProUGUI)dropdown.captionText;

        Resize();

        if (dropdown != null)
            dropdown.onValueChanged.AddListener(_ => Resize());
    }

    private void OnEnable()
    {
        Resize();
    }

    public void Resize()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (label == null)
            return;

        float preferredWidth = label.preferredWidth + padding;
        float width = Mathf.Max(minWidth, preferredWidth);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
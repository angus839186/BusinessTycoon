using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PanelButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;

    public void SetContent(GameObject panel)
    {
        if (panel == null)
            return;

        PanelInfo display =
            panel.GetComponent<PanelInfo>();

        if (labelText != null)
        {
            labelText.text = display != null
                ? display.DisplayName
                : panel.name;
        }

        Sprite icon = display != null ? display.Icon : null;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
    }
}
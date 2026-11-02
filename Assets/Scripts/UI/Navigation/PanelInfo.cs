using UnityEngine;

public sealed class PanelInfo : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName)
            ? gameObject.name
            : displayName;

    public Sprite Icon => icon;
}
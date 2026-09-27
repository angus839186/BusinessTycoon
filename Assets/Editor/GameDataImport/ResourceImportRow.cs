public class ResourceImportRow
{
    public int LineNumber { get; }
    public string ResourceId { get; }
    public string DisplayName { get; }
    public int BasePrice { get; }
    public string IconPath { get; }
    public bool Enabled { get; }

    public ResourceImportRow(
        int lineNumber,
        string resourceId,
        string displayName,
        int basePrice,
        string iconPath,
        bool enabled
    )
    {
        LineNumber = lineNumber;
        ResourceId = resourceId;
        DisplayName = displayName;
        BasePrice = basePrice;
        IconPath = iconPath;
        Enabled = enabled;
    }
}
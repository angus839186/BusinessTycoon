public enum RecipeIoType
{
    Input,
    Output
}
public class RecipeImportRow
{
    public int LineNumber { get; }
    public string OwnerResourceId { get; }
    public RecipeIoType IoType { get; }
    public string ResourceId { get; }
    public int Amount { get; }
    public bool Enabled { get; }

    public RecipeImportRow(
        int lineNumber,
        string ownerResourceId,
        RecipeIoType ioType,
        string resourceId,
        int amount,
        bool enabled
    )
    {
        LineNumber = lineNumber;
        OwnerResourceId = ownerResourceId;
        IoType = ioType;
        ResourceId = resourceId;
        Amount = amount;
        Enabled = enabled;
    }
}
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GameDataTableAttribute : Attribute
{
    public string FileName { get; }
    public string AssetFolder { get; }
    public string IdField { get; }
    public string DisplayName { get; set; }
    public int ImportOrder { get; set; }

    public GameDataTableAttribute(
        string fileName,
        string assetFolder,
        string idField
    )
    {
        FileName = fileName;
        AssetFolder = assetFolder;
        IdField = idField;
    }
}
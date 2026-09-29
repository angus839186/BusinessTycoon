using System;

[AttributeUsage(
    AttributeTargets.Class,
    Inherited = false,
    AllowMultiple = true
)]
public class GameDataColumnAliasAttribute : Attribute
{
    public string Header { get; }
    public string FieldPath { get; }

    public GameDataColumnAliasAttribute(
        string header,
        string fieldPath
    )
    {
        Header = header;
        FieldPath = fieldPath;
    }
}
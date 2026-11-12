using System;

public class ColumnSchema
{
    public string Header { get; }
    public string FieldPath { get; }
    public Type FieldType { get; }

    public bool Required { get; }
    public bool AllowEmpty { get; }

    public double MinValue { get; }
    public double MaxValue { get; }

    public bool HasMinValue =>
        !double.IsNaN(MinValue);

    public bool HasMaxValue =>
        !double.IsNaN(MaxValue);

    public ColumnSchema(
        string header,
        string fieldPath,
        Type fieldType,
        GameDataColumnAttribute attribute
    )
    {
        Header = header;
        FieldPath = fieldPath;
        FieldType = fieldType;

        Required = attribute.Required;
        AllowEmpty = attribute.AllowEmpty;

        MinValue = attribute.MinValue;
        MaxValue = attribute.MaxValue;
    }
}
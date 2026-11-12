using System;

public interface IDataRules
{
    Type DefinitionType { get; }

    void Validate(
        TableSchema schema,
        TsvTable table,
        ValidationResult result
    );
}
using System;

public interface IGameDataSemanticValidator
{
    Type DefinitionType { get; }

    void Validate(
        GameDataTableSchema schema,
        GameDataTsvTable table,
        GameDataValidationResult result
    );
}
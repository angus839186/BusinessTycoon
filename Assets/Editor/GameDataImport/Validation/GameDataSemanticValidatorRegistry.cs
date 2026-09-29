using System;
using System.Collections.Generic;
using UnityEditor;

public static class GameDataSemanticValidatorRegistry
{
    private static Dictionary<Type, IGameDataSemanticValidator>
        validators;

    public static void Validate(
        GameDataTableSchema schema,
        GameDataTsvTable table,
        GameDataValidationResult result
    )
    {
        EnsureInitialized();

        if (validators.TryGetValue(
            schema.DefinitionType,
            out IGameDataSemanticValidator validator
        ))
        {
            validator.Validate(
                schema,
                table,
                result
            );
        }
    }

    private static void EnsureInitialized()
    {
        if (validators != null)
        {
            return;
        }

        validators =
            new Dictionary<Type, IGameDataSemanticValidator>();

        foreach (Type validatorType in
                 TypeCache.GetTypesDerivedFrom<
                     IGameDataSemanticValidator
                 >())
        {
            if (validatorType.IsAbstract ||
                validatorType.IsInterface)
            {
                continue;
            }

            IGameDataSemanticValidator validator =
                Activator.CreateInstance(validatorType)
                as IGameDataSemanticValidator;

            if (validator == null)
            {
                continue;
            }

            if (!validators.TryAdd(
                validator.DefinitionType,
                validator
            ))
            {
                throw new InvalidOperationException(
                    $"Multiple semantic validators exist for " +
                    $"{validator.DefinitionType.Name}."
                );
            }
        }
    }
}
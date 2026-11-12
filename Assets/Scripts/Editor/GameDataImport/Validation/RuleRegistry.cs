using System;
using System.Collections.Generic;
using UnityEditor;

public static class RuleRegistry
{
    private static Dictionary<Type, IDataRules>
        validators;

    public static void Validate(
        TableSchema schema,
        TsvTable table,
        ValidationResult result
    )
    {
        EnsureInitialized();

        if (validators.TryGetValue(
            schema.DefinitionType,
            out IDataRules validator
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
            new Dictionary<Type, IDataRules>();

        foreach (Type validatorType in
                 TypeCache.GetTypesDerivedFrom<
                     IDataRules
                 >())
        {
            if (validatorType.IsAbstract ||
                validatorType.IsInterface)
            {
                continue;
            }

            IDataRules validator =
                Activator.CreateInstance(validatorType)
                as IDataRules;

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
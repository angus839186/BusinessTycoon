using System;
using System.Collections.Generic;
using UnityEngine;

public static class ReferenceResolver
{
    public static bool TryResolve(
        Type definitionType,
        string id,
        out UnityEngine.Object asset,
        out string error
    )
    {
        asset = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(id))
        {
            return true;
        }

        TableSchema schema;

        try
        {
            schema =
                TableSchema.Create(definitionType);
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }

        ImportResult lookupResult =
            new ImportResult();

        Dictionary<string, ScriptableObject> assets =
            AssetRepository.LoadExisting(
                schema,
                lookupResult
            );

        if (!lookupResult.IsSuccess)
        {
            error = string.Join(
                "\n",
                lookupResult.Errors
            );

            return false;
        }

        if (!assets.TryGetValue(id, out ScriptableObject result))
        {
            error =
                $"{definitionType.Name} with ID '{id}' " +
                "was not found.";

            return false;
        }

        asset = result;
        return true;
    }
}
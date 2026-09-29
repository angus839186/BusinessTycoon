using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameDataObjectReferenceResolver
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

        GameDataTableSchema schema;

        try
        {
            schema =
                GameDataTableSchema.Create(definitionType);
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }

        GameDataImportResult lookupResult =
            new GameDataImportResult();

        Dictionary<string, ScriptableObject> assets =
            GameDataAssetRepository.LoadExisting(
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
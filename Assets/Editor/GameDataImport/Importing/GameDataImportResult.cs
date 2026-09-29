using System.Collections.Generic;

public class GameDataImportResult
{
    public IReadOnlyList<string> Messages => messages;
    public IReadOnlyList<string> Errors => errors;
    public IReadOnlyList<string> Warnings => warnings;
    public int WarningCount => warnings.Count;

    public int CreatedCount { get; private set; }
    public int UpdatedCount { get; private set; }

    public bool IsSuccess => errors.Count == 0;

    private readonly List<string> messages =
        new List<string>();

    private readonly List<string> warnings =
new List<string>();

    private readonly List<string> errors =
        new List<string>();

    public void AddCreated(string id)
    {
        CreatedCount++;
        messages.Add($"Created: {id}");
    }

    public void AddUpdated(string id)
    {
        UpdatedCount++;
        messages.Add($"Updated: {id}");
    }

    public void AddError(string error)
    {
        errors.Add(error);
    }
    public void AddWarning(string warning)
    {
        warnings.Add(warning);
    }
}
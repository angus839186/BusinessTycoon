using System.Collections.Generic;

public class GameDataValidationResult
{
    public IReadOnlyList<GameDataValidationMessage> Messages =>
        messages;

    public int ErrorCount { get; private set; }
    public int WarningCount { get; private set; }

    public bool IsValid => ErrorCount == 0;

    private readonly List<GameDataValidationMessage> messages =
        new List<GameDataValidationMessage>();

    public void AddError(
        int lineNumber,
        string header,
        string message
    )
    {
        messages.Add(
            new GameDataValidationMessage(
                GameDataValidationSeverity.Error,
                lineNumber,
                header,
                message
            )
        );

        ErrorCount++;
    }

    public void AddWarning(
        int lineNumber,
        string header,
        string message
    )
    {
        messages.Add(
            new GameDataValidationMessage(
                GameDataValidationSeverity.Warning,
                lineNumber,
                header,
                message
            )
        );

        WarningCount++;
    }
}
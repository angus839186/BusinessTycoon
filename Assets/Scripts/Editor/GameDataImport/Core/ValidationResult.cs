using System.Collections.Generic;

public class ValidationResult
{
    public IReadOnlyList<ValidationMessage> Messages =>
        messages;

    public int ErrorCount { get; private set; }
    public int WarningCount { get; private set; }

    public bool IsValid => ErrorCount == 0;

    private readonly List<ValidationMessage> messages =
        new List<ValidationMessage>();

    public void AddError(
        int lineNumber,
        string header,
        string message
    )
    {
        messages.Add(
            new ValidationMessage(
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
            new ValidationMessage(
                GameDataValidationSeverity.Warning,
                lineNumber,
                header,
                message
            )
        );

        WarningCount++;
    }
}
public enum GameDataValidationSeverity
{
    Warning,
    Error
}

public class GameDataValidationMessage
{
    public GameDataValidationSeverity Severity { get; }
    public int LineNumber { get; }
    public string Header { get; }
    public string Message { get; }

    public GameDataValidationMessage(
        GameDataValidationSeverity severity,
        int lineNumber,
        string header,
        string message
    )
    {
        Severity = severity;
        LineNumber = lineNumber;
        Header = header;
        Message = message;
    }

    public override string ToString()
    {
        string location = LineNumber > 0
            ? $"Line {LineNumber}"
            : "Table";

        if (!string.IsNullOrWhiteSpace(Header))
        {
            location += $", {Header}";
        }

        return $"{location}: {Message}";
    }
}
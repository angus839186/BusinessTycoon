using System.Collections.Generic;

public class GameDataTsvRow
{
    public int LineNumber { get; }

    public IReadOnlyDictionary<string, string> Cells =>
        cells;

    private readonly Dictionary<string, string> cells;

    public GameDataTsvRow(
        int lineNumber,
        Dictionary<string, string> cells
    )
    {
        LineNumber = lineNumber;
        this.cells = cells;
    }

    public bool TryGetValue(string header, out string value)
    {
        return cells.TryGetValue(header, out value);
    }

    public string GetValue(string header)
    {
        return cells.TryGetValue(header, out string value)
            ? value
            : string.Empty;
    }
}
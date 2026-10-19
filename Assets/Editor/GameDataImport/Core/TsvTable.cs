
using System.Collections.Generic;

public class TsvTable
{
    public string SourcePath { get; }

    public IReadOnlyList<string> Headers =>
        headers;

    public IReadOnlyList<TsvRow> Rows =>
        rows;

    private readonly List<string> headers;
    private readonly List<TsvRow> rows;

    public TsvTable(
        string sourcePath,
        IReadOnlyList<string> headers,
        IReadOnlyList<TsvRow> rows
    )
    {
        SourcePath = sourcePath;
        this.headers = new List<string>(headers);
        this.rows = new List<TsvRow>(rows);
    }
}

using System.Collections.Generic;

public class GameDataTsvTable
{
    public string SourcePath { get; }

    public IReadOnlyList<string> Headers =>
        headers;

    public IReadOnlyList<GameDataTsvRow> Rows =>
        rows;

    private readonly List<string> headers;
    private readonly List<GameDataTsvRow> rows;

    public GameDataTsvTable(
        string sourcePath,
        IReadOnlyList<string> headers,
        IReadOnlyList<GameDataTsvRow> rows
    )
    {
        SourcePath = sourcePath;
        this.headers = new List<string>(headers);
        this.rows = new List<GameDataTsvRow>(rows);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class TsvReader
{
    public static TsvTable Read(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException(
                "TSV source path is empty.",
                nameof(sourcePath)
            );
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(
                $"TSV file was not found: {sourcePath}",
                sourcePath
            );
        }

        string[] lines = File.ReadAllLines(
            sourcePath,
            Encoding.UTF8
        );

        if (lines.Length == 0)
        {
            throw new InvalidDataException(
                $"TSV file is empty: {sourcePath}"
            );
        }

        string headerLine = lines[0].TrimStart('\uFEFF');
        string[] headers = headerLine.Split('\t');

        ValidateHeaders(headers, sourcePath);

        List<TsvRow> rows =
            new List<TsvRow>();

        for (int index = 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                continue;
            }

            string[] values = lines[index].Split('\t');
            int lineNumber = index + 1;

            if (values.Length != headers.Length)
            {
                throw new InvalidDataException(
                    $"{sourcePath} line {lineNumber} has " +
                    $"{values.Length} columns, expected " +
                    $"{headers.Length}."
                );
            }

            Dictionary<string, string> cells =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase
                );

            for (int column = 0; column < headers.Length; column++)
            {
                cells.Add(
                    headers[column],
                    values[column].Trim()
                );
            }

            rows.Add(
                new TsvRow(
                    lineNumber,
                    cells
                )
            );
        }

        return new TsvTable(
            sourcePath,
            headers,
            rows
        );
    }

    private static void ValidateHeaders(
        string[] headers,
        string sourcePath
    )
    {
        HashSet<string> uniqueHeaders =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        for (int index = 0; index < headers.Length; index++)
        {
            headers[index] = headers[index].Trim();

            if (string.IsNullOrWhiteSpace(headers[index]))
            {
                throw new InvalidDataException(
                    $"{sourcePath} contains an empty header " +
                    $"at column {index + 1}."
                );
            }

            if (!uniqueHeaders.Add(headers[index]))
            {
                throw new InvalidDataException(
                    $"{sourcePath} contains duplicate header " +
                    $"'{headers[index]}'."
                );
            }
        }
    }
}
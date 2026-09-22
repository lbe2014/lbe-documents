namespace Lbe.Documents;

/// <summary>A title, column headings, and rows that can be exported to several formats.</summary>
public sealed class TabularReport
{
    /// <summary>Report heading.</summary>
    public string Title { get; }
    /// <summary>Column labels, in display order.</summary>
    public IReadOnlyList<string> Columns { get; }
    /// <summary>Data rows; each row has one cell per column.</summary>
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }

    /// <summary>Creates an immutable snapshot of the supplied report data.</summary>
    public TabularReport(string title, IEnumerable<string> columns, IEnumerable<IEnumerable<string>> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);

        Title = title.Trim();
        Columns = Array.AsReadOnly(columns.Select(value => value ?? throw new ArgumentException("Columns cannot contain null.", nameof(columns))).ToArray());
        if (Columns.Count == 0 || Columns.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("At least one non-empty column heading is required.", nameof(columns));

        var snapshot = rows.Select(row =>
        {
            ArgumentNullException.ThrowIfNull(row);
            var values = row.Select(value => value ?? throw new ArgumentException("Rows cannot contain null cells.", nameof(rows))).ToArray();
            if (values.Length != Columns.Count)
                throw new ArgumentException("Every row must have the same number of cells as the column headings.", nameof(rows));
            return (IReadOnlyList<string>)Array.AsReadOnly(values);
        }).ToArray();
        Rows = Array.AsReadOnly(snapshot);
    }
}

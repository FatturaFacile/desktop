// <copyright file="CsvBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;

namespace FatturaFacile.Application.Common.Export;

/// <summary>
/// Builds a semicolon separated file readable by Excel in Switzerland and Italy.
/// </summary>
public sealed class CsvBuilder
{
    private static readonly char[] SpecialCharacters = [';', '"', '\r', '\n'];
    private readonly StringBuilder _builder = new();

    /// <summary>
    /// Appends a row.
    /// </summary>
    /// <param name="values">The cell values.</param>
    public void Row(params object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _ = _builder.AppendJoin(';', values.Select(static v => Quote(Format(v))));
        _ = _builder.Append("\r\n");
    }

    /// <inheritdoc />
    public override string ToString() => _builder.ToString();

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime date when date.TimeOfDay == TimeSpan.Zero => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime date => date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        bool flag => flag ? "1" : "0",
        IFormattable formattable => formattable.ToString(format: null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static string Quote(string text) => text.IndexOfAny(SpecialCharacters) < 0
        ? text
        : "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

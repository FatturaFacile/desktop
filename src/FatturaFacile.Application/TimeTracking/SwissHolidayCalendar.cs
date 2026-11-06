// <copyright file="SwissHolidayCalendar.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Default public holidays by canton, used to create the editable company calendar. Ticino is complete; for the other
/// cantons only the holidays common to most of them are proposed and the calendar must be checked.
/// </summary>
public static class SwissHolidayCalendar
{
    /// <summary>
    /// Returns the default holidays of a canton for a year.
    /// </summary>
    /// <param name="canton">The canton.</param>
    /// <param name="year">The year.</param>
    /// <returns>The holidays.</returns>
    public static IReadOnlyList<(DateTime Date, string Name)> ForCanton(string? canton, int year)
    {
        var easter = EasterSunday(year);
        List<(DateTime Date, string Name)> days =
        [
            (Day(year, 1, 1), "Capodanno"),
            (easter.AddDays(1), "Lunedì di Pasqua"),
            (easter.AddDays(39), "Ascensione"),
            (easter.AddDays(50), "Lunedì di Pentecoste"),
            (Day(year, 8, 1), "Festa nazionale"),
            (Day(year, 12, 25), "Natale"),
            (Day(year, 12, 26), "Santo Stefano"),
        ];

        if (string.Equals(canton, "TI", StringComparison.OrdinalIgnoreCase))
        {
            days.AddRange(
            [
                (Day(year, 1, 6), "Epifania"),
                (Day(year, 3, 19), "San Giuseppe"),
                (Day(year, 5, 1), "Festa del lavoro"),
                (easter.AddDays(60), "Corpus Domini"),
                (Day(year, 6, 29), "SS. Pietro e Paolo"),
                (Day(year, 8, 15), "Assunzione"),
                (Day(year, 11, 1), "Ognissanti"),
                (Day(year, 12, 8), "Immacolata"),
            ]);
        }
        else
        {
            days.Add((easter.AddDays(-2), "Venerdì Santo"));
        }

        return [.. days.OrderBy(static d => d.Date)];
    }

    /// <summary>
    /// Returns Easter Sunday (Gregorian computus).
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>The date of Easter.</returns>
    public static DateTime EasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = ((19 * a) + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + (2 * e) + (2 * i) - h - k) % 7;
        var m = (a + (11 * h) + (22 * l)) / 451;
        var month = (h + l - (7 * m) + 114) / 31;
        var day = ((h + l - (7 * m) + 114) % 31) + 1;
        return Day(year, month, day);
    }

    private static DateTime Day(int year, int month, int dayOfMonth) => new(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Unspecified);
}

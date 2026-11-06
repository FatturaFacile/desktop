// <copyright file="SwissHolidayCalendar.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Public holidays of the Swiss cantons (the national day plus the cantonal holidays equal to Sundays, art. 20a LL).
/// The lists cover the holidays valid on the whole territory of each canton; holidays observed only in some municipalities
/// or as half days can be added or corrected in the holiday table of the company.
/// </summary>
public static class SwissHolidayCalendar
{
    private static readonly string[] AllCantons =
        ["AG", "AI", "AR", "BE", "BL", "BS", "FR", "GE", "GL", "GR", "JU", "LU", "NE", "NW", "OW", "SG", "SH", "SO", "SZ", "TG", "TI", "UR", "VD", "VS", "ZG", "ZH"];

    private static readonly string[] Berchtold = ["AG", "BE", "FR", "GL", "JU", "LU", "NE", "OW", "SH", "SO", "TG", "VD", "ZG", "ZH"];
    private static readonly string[] Epiphany = ["SZ", "TI", "UR"];
    private static readonly string[] SaintJoseph = ["NW", "SZ", "TI", "UR", "VS"];
    private static readonly string[] WithoutGoodFriday = ["TI", "VS"];
    private static readonly string[] WithoutEasterMonday = ["VS"];
    private static readonly string[] LabourDay = ["BL", "BS", "JU", "NE", "SH", "TG", "TI", "ZH"];
    private static readonly string[] CorpusChristi = ["AI", "FR", "JU", "LU", "NW", "OW", "SO", "SZ", "TI", "UR", "VS", "ZG"];
    private static readonly string[] Assumption = ["AI", "FR", "JU", "LU", "NW", "OW", "SO", "SZ", "TI", "UR", "VS", "ZG"];
    private static readonly string[] AllSaints = ["AI", "FR", "GL", "JU", "LU", "NW", "OW", "SG", "SO", "SZ", "TI", "UR", "VS", "ZG"];
    private static readonly string[] ImmaculateConception = ["AI", "FR", "LU", "NW", "OW", "SZ", "TI", "UR", "VS", "ZG"];
    private static readonly string[] WithoutSaintStephen = ["GE", "JU", "NE", "VD", "VS"];

    /// <summary>
    /// Gets the canton codes.
    /// </summary>
    public static IReadOnlyList<string> Cantons => AllCantons;

    /// <summary>
    /// Gets the public holidays of a canton.
    /// </summary>
    /// <param name="canton">The canton code (Ticino when empty).</param>
    /// <param name="year">The year.</param>
    /// <returns>The holidays ordered by date.</returns>
    public static IReadOnlyList<(DateTime Date, string Name)> ForCanton(string? canton, int year)
    {
        var code = string.IsNullOrWhiteSpace(canton) ? "TI" : canton.Trim().ToUpperInvariant();
        var easter = EasterSunday(year);
        List<(DateTime Date, string Name)> days =
        [
            (Day(year, 1, 1), "Capodanno"),
            (easter.AddDays(39), "Ascensione"),
            (Day(year, 8, 1), "Festa nazionale"),
            (Day(year, 12, 25), "Natale"),
        ];

        AddIf(days, Berchtold.Contains(code), Day(year, 1, 2), "San Bertoldo");
        AddIf(days, Epiphany.Contains(code), Day(year, 1, 6), "Epifania");
        AddIf(days, string.Equals(code, "NE", StringComparison.Ordinal), Day(year, 3, 1), "Instaurazione della Repubblica");
        AddIf(days, SaintJoseph.Contains(code), Day(year, 3, 19), "San Giuseppe");
        AddIf(days, !WithoutGoodFriday.Contains(code), easter.AddDays(-2), "Venerdì Santo");
        AddIf(days, !WithoutEasterMonday.Contains(code), easter.AddDays(1), "Lunedì di Pasqua");
        AddIf(days, string.Equals(code, "GL", StringComparison.Ordinal), NaefelserFahrt(year, easter), "Näfelser Fahrt");
        AddIf(days, LabourDay.Contains(code), Day(year, 5, 1), "Festa del lavoro");
        AddIf(days, !WithoutEasterMonday.Contains(code), easter.AddDays(50), "Lunedì di Pentecoste");
        AddIf(days, CorpusChristi.Contains(code), easter.AddDays(60), "Corpus Domini");
        AddIf(days, string.Equals(code, "JU", StringComparison.Ordinal), Day(year, 6, 23), "Plebiscito giurassiano");
        AddIf(days, string.Equals(code, "TI", StringComparison.Ordinal), Day(year, 6, 29), "SS. Pietro e Paolo");
        AddIf(days, Assumption.Contains(code), Day(year, 8, 15), "Assunzione");
        AddIf(days, string.Equals(code, "GE", StringComparison.Ordinal), FirstSundayOfSeptember(year).AddDays(4), "Digiuno ginevrino");
        AddIf(days, string.Equals(code, "VD", StringComparison.Ordinal), FirstSundayOfSeptember(year).AddDays(15), "Lunedì del Digiuno federale");
        AddIf(days, string.Equals(code, "OW", StringComparison.Ordinal), Day(year, 9, 25), "San Nicolao della Flüe");
        AddIf(days, AllSaints.Contains(code), Day(year, 11, 1), "Ognissanti");
        AddIf(days, ImmaculateConception.Contains(code), Day(year, 12, 8), "Immacolata");
        AddIf(days, !WithoutSaintStephen.Contains(code), Day(year, 12, 26), "Santo Stefano");
        AddIf(days, string.Equals(code, "GE", StringComparison.Ordinal), Day(year, 12, 31), "Restaurazione della Repubblica");

        return [.. days.OrderBy(static d => d.Date)];
    }

    /// <summary>
    /// Computes Easter Sunday (Gregorian calendar, anonymous algorithm).
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>Easter Sunday.</returns>
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

    private static void AddIf(List<(DateTime Date, string Name)> days, bool condition, DateTime date, string name)
    {
        if (condition)
        {
            days.Add((date, name));
        }
    }

    private static DateTime FirstSundayOfSeptember(int year)
    {
        var first = Day(year, 9, 1);
        return first.AddDays(((int)DayOfWeek.Sunday - (int)first.DayOfWeek + 7) % 7);
    }

    private static DateTime NaefelserFahrt(int year, DateTime easter)
    {
        var first = Day(year, 4, 1);
        var thursday = first.AddDays(((int)DayOfWeek.Thursday - (int)first.DayOfWeek + 7) % 7);

        // Moved by one week when it falls in Holy Week.
        return thursday > easter.AddDays(-7) && thursday < easter ? thursday.AddDays(7) : thursday;
    }

    private static DateTime Day(int year, int month, int dayOfMonth) => new(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Unspecified);
}

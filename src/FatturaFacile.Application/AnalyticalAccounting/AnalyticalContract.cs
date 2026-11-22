// <copyright file="AnalyticalContract.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The enum values of the industrial and analytical cost accounting contract (Mod 018), identical on the web-api, the web
/// frontend, the desktop and the mobile client.
/// </summary>
public static class AnalyticalContract
{
    /// <summary>
    /// The snapshot key of the center tree.
    /// </summary>
    public const string CentersKey = "centers";

    /// <summary>
    /// The snapshot key of the apportionment runs.
    /// </summary>
    public const string RunsKey = "runs";

    /// <summary>
    /// The snapshot key of the contribution margin statement.
    /// </summary>
    public const string MarginKey = "margin";

    /// <summary>
    /// The snapshot key of the direct-versus-apportioned comparison.
    /// </summary>
    public const string ComparisonKey = "comparison";

    /// <summary>
    /// The center types.
    /// </summary>
    public static readonly IReadOnlyList<string> CenterTypes = ["COST_CENTER", "PROFIT_CENTER"];

    /// <summary>
    /// The center categories.
    /// </summary>
    public static readonly IReadOnlyList<string> CenterCategories = ["STRUCTURAL", "AUXILIARY", "PRODUCTION", "LOGISTICS", "COMMERCIAL", "PROFIT"];

    /// <summary>
    /// The allocation driver types.
    /// </summary>
    public static readonly IReadOnlyList<string> DriverTypes = ["AREA_SQM", "HEADCOUNT_FTE", "MACHINE_HOURS", "REVENUE_VOLUME", "FIXED_PERCENTAGE", "CUSTOM"];

    /// <summary>
    /// The cost element types.
    /// </summary>
    public static readonly IReadOnlyList<string> CostElementTypes = ["VARIABLE_DIRECT", "FIXED_DIRECT", "INDIRECT_AUXILIARY", "REVENUE", "NON_OPERATING"];

    /// <summary>
    /// The statuses of an apportionment run.
    /// </summary>
    public static readonly IReadOnlyList<string> RunStatuses = ["PREVIEW", "CONSOLIDATED", "REVERSED"];
}

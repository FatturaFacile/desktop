// <copyright file="TimeRecordingMode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// The working time recording modes allowed by the Swiss ordinance 1 on the Labour Act (values of HrEmployee.TimeRecordingMode).
/// </summary>
public static class TimeRecordingMode
{
    /// <summary>
    /// Full recording (art. 73 OLL 1): start and end of work, breaks of 30 minutes or more, daily and weekly duration.
    /// </summary>
    public const string Detailed = "Detailed";

    /// <summary>
    /// Simplified recording (art. 73b OLL 1): only the daily duration, plus start and end of night and Sunday work.
    /// Requires an agreement with the employee representatives (or an individual written agreement in companies with
    /// fewer than 50 employees) and a yearly workload interview.
    /// </summary>
    public const string Simplified = "Simplified";

    /// <summary>
    /// Waiver (art. 73a OLL 1): only with a collective labour agreement, great autonomy, annual gross salary above the
    /// threshold and an individual written agreement, revocable every year.
    /// </summary>
    public const string Waived = "Waived";

    /// <summary>
    /// Gets all the modes.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = [Detailed, Simplified, Waived];
}

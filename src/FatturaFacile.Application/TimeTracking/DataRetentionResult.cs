// <copyright file="DataRetentionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Outcome of the retention rules for one company.
/// </summary>
/// <param name="TenantId">The company.</param>
/// <param name="Cutoff">Records dated before this day were deleted.</param>
/// <param name="ClockEvents">The deleted clock events.</param>
/// <param name="DailyRecords">The deleted daily records.</param>
/// <param name="AuditEntries">The deleted audit entries.</param>
public sealed record DataRetentionResult(Guid TenantId, DateTime Cutoff, int ClockEvents, int DailyRecords, int AuditEntries)
{
    /// <summary>
    /// Gets the number of deleted records.
    /// </summary>
    public int Total => ClockEvents + DailyRecords + AuditEntries;
}

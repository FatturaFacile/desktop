// <copyright file="DataRetentionStatusDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Retention settings of the active company.
/// </summary>
/// <param name="Years">The number of years the records are kept.</param>
/// <param name="Cutoff">Records dated before this day are deleted.</param>
/// <param name="LastRunAt">When the rules were last applied (UTC).</param>
public sealed record DataRetentionStatusDto(int Years, DateTime Cutoff, DateTime? LastRunAt);

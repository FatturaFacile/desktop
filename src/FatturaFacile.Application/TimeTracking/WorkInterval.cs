// <copyright file="WorkInterval.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// A continuous working period between two clock events.
/// </summary>
/// <param name="Start">The start.</param>
/// <param name="End">The end.</param>
public sealed record WorkInterval(DateTime Start, DateTime End)
{
    /// <summary>
    /// Gets the duration in minutes.
    /// </summary>
    public int Minutes => (int)Math.Round((End - Start).TotalMinutes, MidpointRounding.AwayFromZero);
}

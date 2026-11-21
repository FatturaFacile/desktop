// <copyright file="ManualTimeProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
#pragma warning disable SA1600 // Elements should be documented: internal test double.

namespace FatturaFacile.UnitTests;

/// <summary>
/// A clock that only moves when the test advances it.
/// </summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset now = new(2026, 11, 21, 9, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => now;

    public void Advance(TimeSpan by) => now += by;
}

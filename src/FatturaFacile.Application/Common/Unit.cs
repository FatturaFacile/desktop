// <copyright file="Unit.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common;

/// <summary>
/// Represents a void or empty return value for commands and operations that produce no functional result value.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the singleton default instance of the <see cref="Unit"/> structure.
    /// </summary>
    public static readonly Unit Value = new();

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public static bool operator ==(Unit left, Unit right) => true;

    /// <inheritdoc />
    public static bool operator !=(Unit left, Unit right) => false;
}

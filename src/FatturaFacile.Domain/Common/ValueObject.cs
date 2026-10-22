// <copyright file="ValueObject.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Provides the base class for Domain-Driven Design Value Objects whose equality is governed by structural components rather than identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Evaluates whether two value objects possess identical structural components.
    /// </summary>
    /// <param name="left">The first value object operand to compare.</param>
    /// <param name="right">The second value object operand to compare.</param>
    /// <returns><see langword="true"/> if both instances share equal component values; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(
        ValueObject? left,
        ValueObject? right) => Equals(
        left,
        right);

    /// <summary>
    /// Evaluates whether two value objects differ in any structural component.
    /// </summary>
    /// <param name="left">The first value object operand to compare.</param>
    /// <param name="right">The second value object operand to compare.</param>
    /// <returns><see langword="true"/> if the instances differ or if one is <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(
        ValueObject? left,
        ValueObject? right) => !Equals(
        left,
        right);

    /// <inheritdoc/>
    public override bool Equals(
        object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Indicates whether the current value object possesses equal constituent values to another instance of the exact same type.
    /// </summary>
    /// <param name="other">A value object instance to compare with this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> is of the same runtime type and yields an equivalent component sequence; otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(
        ValueObject? other) => other is not null && other.GetType() == GetType() &&
                               GetEqualityComponents()
                                   .SequenceEqual(other.GetEqualityComponents());

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                default(int),
                HashCode.Combine);
    }

    /// <summary>
    /// Yields the atomic property values and elements that define structural equality for this value object.
    /// </summary>
    /// <returns>An enumerable sequence containing all atomic constituent values in deterministic order.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}

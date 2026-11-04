// <copyright file="ValueObject.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Primitives;

/// <summary>
/// Base class for domain value objects that are evaluated by structural equality of their components rather than identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Determines whether the specified object is structurally equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /// <summary>
    /// Serves as the hash function for the value object, combining the hashes of all atomic components.
    /// </summary>
    /// <returns>A hash code for the current value object.</returns>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Yields the atomic values that uniquely define the identity and equality of this value object.
    /// </summary>
    /// <returns>An enumerable sequence of individual atomic property values.</returns>
    protected abstract IEnumerable<object?> GetAtomicValues();
}

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Base class for Domain-Driven Design Value Objects providing structural equality semantics.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Yields the atomic components that define value object equality.
    /// </summary>
    /// <returns>An enumerable sequence of equality component objects.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

/// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

/// <summary>
    /// Indicates whether the current value object is equal to another value object of the same type.
    /// </summary>
    /// <param name="other">A value object to compare with this instance.</param>
    /// <returns><see langword="true"/> if the current value object is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(ValueObject? other) => other is not null && other.GetType() == GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

/// <inheritdoc/>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(default(int), HashCode.Combine);
    }

/// <summary>
    /// Determines whether two value objects are structurally equal.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

/// <summary>
    /// Determines whether two value objects are not structurally equal.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns><see langword="true"/> if not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}

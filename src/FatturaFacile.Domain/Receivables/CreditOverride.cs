// <copyright file="CreditOverride.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// An authorised exception that lets operations through despite a credit breach.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Scope">The operations it covers.</param>
/// <param name="Reason">The reason given.</param>
/// <param name="AuthorizedBy">Who authorised it.</param>
/// <param name="ExpiresAt">When it expires, when it does.</param>
/// <param name="RevokedAt">When it was revoked, when it was.</param>
/// <param name="RevokeReason">The reason of the revocation.</param>
/// <param name="CreatedAt">When it was granted.</param>
public sealed record CreditOverride(
    Guid Id,
    Guid CustomerId,
    CreditScope Scope,
    string Reason,
    string? AuthorizedBy,
    DateTime? ExpiresAt,
    DateTime? RevokedAt,
    string? RevokeReason,
    DateTime? CreatedAt)
{
    /// <summary>
    /// Gets a value indicating whether the override is in force at an instant.
    /// </summary>
    /// <param name="utcNow">The instant.</param>
    /// <returns>True when not revoked and not expired.</returns>
    public bool IsActiveAt(DateTime utcNow) => RevokedAt is null && (ExpiresAt is null || ExpiresAt > utcNow);

    /// <summary>
    /// Gets a value indicating whether the override covers an operation.
    /// </summary>
    /// <param name="scope">The operation.</param>
    /// <returns>True for an override on everything or on that operation.</returns>
    public bool Covers(CreditScope scope) => Scope == CreditScope.All || Scope == scope;
}

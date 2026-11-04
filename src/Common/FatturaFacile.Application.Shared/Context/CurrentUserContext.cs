// <copyright file="CurrentUserContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Shared.Context;

/// <summary>
/// Default ambient implementation of <see cref="ICurrentUserContext"/>.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    /// <inheritdoc />
    public Guid? UserId { get; set; }

    /// <inheritdoc />
    public Guid? TenantId { get; set; }

    /// <inheritdoc />
    public string Culture { get; set; } = "en-US";
}

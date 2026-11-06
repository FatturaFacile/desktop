// <copyright file="IAuthorizedRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Common.Security;

/// <summary>
/// Marks a MediatR request that requires a permission in the current tenant (enforced by the authorization behavior).
/// </summary>
public interface IAuthorizedRequest
{
    /// <summary>
    /// Gets the permission required to execute the request.
    /// </summary>
    public Permission RequiredPermission { get; }
}

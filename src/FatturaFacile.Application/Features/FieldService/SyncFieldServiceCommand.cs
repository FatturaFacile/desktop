// <copyright file="SyncFieldServiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Pulls one Field Service read model from the server into the local cache; a failure leaves the cache untouched.
/// </summary>
/// <param name="Scope">The read model to pull.</param>
/// <param name="Key">The work order, equipment or technician identifier, for the scopes that need one.</param>
/// <param name="From">The start of the board window, or null for the current week.</param>
/// <param name="To">The end of the board window, or null.</param>
public sealed record SyncFieldServiceCommand(FieldServiceSyncScope Scope, string? Key, DateTimeOffset? From, DateTimeOffset? To) : IRequest<FieldServiceCallResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}

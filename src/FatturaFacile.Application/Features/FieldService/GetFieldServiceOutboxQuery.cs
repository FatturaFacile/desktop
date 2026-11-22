// <copyright file="GetFieldServiceOutboxQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the state of the outbox of field operations: how many wait to be sent and which ones the server rejected.
/// </summary>
public sealed record GetFieldServiceOutboxQuery : IRequest<FieldServiceOutboxSummary>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}

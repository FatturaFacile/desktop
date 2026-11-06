// <copyright file="DeleteTimeEventCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Deletes a clock event logically (it stays in the database with the reason, visible in the audit trail).
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="Reason">The reason of the deletion.</param>
public sealed record DeleteTimeEventCommand(Guid Id, string Reason) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeManage;
}

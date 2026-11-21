// <copyright file="QueueOperationConfirmationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Queues an operation confirmation (shop-floor posting).
/// </summary>
/// <param name="Input">The posting.</param>
public sealed record QueueOperationConfirmationCommand(OperationConfirmationInput Input) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingWrite;
}

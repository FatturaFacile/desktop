// <copyright file="QueueOrderTransitionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Queues a release, start, complete or close of a production order.
/// </summary>
/// <param name="OrderId">The local order identifier.</param>
/// <param name="Kind">The transition.</param>
public sealed record QueueOrderTransitionCommand(Guid OrderId, ManufacturingMutationKind Kind) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingWrite;
}

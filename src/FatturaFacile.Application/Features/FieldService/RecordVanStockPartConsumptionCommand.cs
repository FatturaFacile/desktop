// <copyright file="RecordVanStockPartConsumptionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Records a spare part taken from the technician van. It is saved in the local outbox first and sent with an idempotency key, so it works offline and is applied once.
/// </summary>
/// <param name="Part">The part.</param>
public sealed record RecordVanStockPartConsumptionCommand(FieldServicePartInput Part) : IRequest<FieldServiceQueueResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}

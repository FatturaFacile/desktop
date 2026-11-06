// <copyright file="SaveTimeDailyRecordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Records the daily working time of an employee with simplified recording (art. 73b OLL 1).
/// </summary>
/// <param name="Record">The record.</param>
/// <param name="Reason">The reason when an existing record is corrected.</param>
public sealed record SaveTimeDailyRecordCommand(TimeDailyRecord Record, string? Reason) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}

// <copyright file="QueueAnalyticalEntryCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.AnalyticalAccounting;

/// <summary>
/// Validates a manual analytical entry (with its allocation vectors) and queues it for the web-api, offline first.
/// </summary>
/// <param name="Draft">The entry.</param>
public sealed record QueueAnalyticalEntryCommand(AnalyticalEntryDraft Draft) : IRequest<QueueAnalyticalEntryResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

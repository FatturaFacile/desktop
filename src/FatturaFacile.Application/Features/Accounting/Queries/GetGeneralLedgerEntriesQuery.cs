// <copyright file="GetGeneralLedgerEntriesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Query to retrieve General Ledger journal entries (Prima Nota) for the active tenant.
/// </summary>
public sealed record GetGeneralLedgerEntriesQuery : IRequest<IReadOnlyList<GeneralLedgerEntryDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}

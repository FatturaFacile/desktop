// <copyright file="PostJournalEntryCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Posts a draft Prima Nota entry. Once posted it can only be reversed.
/// </summary>
/// <param name="EntryId">The draft entry.</param>
public sealed record PostJournalEntryCommand(Guid EntryId) : IRequest<GeneralLedgerEntryDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

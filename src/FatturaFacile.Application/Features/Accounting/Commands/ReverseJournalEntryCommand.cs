// <copyright file="ReverseJournalEntryCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Reverses (storna) a posted entry by creating a linked counter-entry with swapped sides. The original is never modified.
/// </summary>
/// <param name="EntryId">The posted entry to reverse.</param>
/// <param name="ReversalDate">The posting date of the counter-entry.</param>
/// <param name="Reason">The reason of the storno.</param>
public sealed record ReverseJournalEntryCommand(Guid EntryId, DateTime ReversalDate, string? Reason) : IRequest<GeneralLedgerEntryDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

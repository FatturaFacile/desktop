// <copyright file="CreateJournalEntryCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Creates a Prima Nota entry in a ledger book, as a draft or posted straight away. A posted entry must balance; the numbering is automatic.
/// </summary>
/// <param name="PostingDate">The accounting posting date.</param>
/// <param name="Description">The narrative.</param>
/// <param name="LedgerType">The ledger book.</param>
/// <param name="Lines">The debit and credit lines.</param>
/// <param name="PostImmediately">Whether to post (lock) the entry; false keeps it a draft.</param>
/// <param name="SourceDocumentType">The source classification, "manual" by default.</param>
public sealed record CreateJournalEntryCommand(
    DateTime PostingDate,
    string Description,
    LedgerBook LedgerType,
    IReadOnlyList<JournalLineInput> Lines,
    bool PostImmediately,
    string SourceDocumentType = "manual") : IRequest<GeneralLedgerEntryDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

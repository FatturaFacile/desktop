// <copyright file="CreateGeneralLedgerEntryCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Command to create a balanced double-entry General Ledger voucher (Prima Nota).
/// </summary>
public sealed class CreateGeneralLedgerEntryCommand : IRequest<GeneralLedgerEntryDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;

    /// <summary>
    /// Gets or sets the entry voucher number.
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the posting date.
    /// </summary>
    public DateTime PostingDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the document date.
    /// </summary>
    public DateTime DocumentDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets narrative description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets source document type ("invoice", "bank", "manual").
    /// </summary>
    public string? SourceDocumentType { get; set; }

    /// <summary>
    /// Gets or sets reference id.
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets debit and credit lines.
    /// </summary>
    public IList<GeneralLedgerLineDto> Lines { get; set; } = [];
}

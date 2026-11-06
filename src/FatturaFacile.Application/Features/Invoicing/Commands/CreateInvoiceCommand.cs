// <copyright file="CreateInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Command to create a new commercial invoice.
/// </summary>
public sealed class CreateInvoiceCommand : IRequest<InvoiceDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;

    /// <summary>
    /// Gets or sets recipient contact id.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets invoice document number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets issue date.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets payment due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets currency code.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Italian split payment applies.
    /// </summary>
    public bool SplitPayment { get; set; }

    /// <summary>
    /// Gets or sets the invoice type ("standard", "credit_note", "debit_note", "proforma").
    /// </summary>
    public string InvoiceType { get; set; } = "standard";

    /// <summary>
    /// Gets or sets a value indicating whether the invoice is subject to reverse charge (inversione contabile).
    /// </summary>
    public bool ReverseCharge { get; set; }

    /// <summary>
    /// Gets or sets the Italian withholding tax rate in percent (ritenuta d'acconto); null when not applicable.
    /// </summary>
    public decimal? WithholdingTaxPercent { get; set; }

    /// <summary>
    /// Gets or sets line items for the invoice.
    /// </summary>
    public IList<InvoiceItemDto> Items { get; set; } = [];
}

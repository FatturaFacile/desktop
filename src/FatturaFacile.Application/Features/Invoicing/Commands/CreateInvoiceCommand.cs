// <copyright file="CreateInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Command to create a new commercial invoice.
/// </summary>
public sealed class CreateInvoiceCommand : IRequest<InvoiceDto>
{
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
    /// Gets or sets line items for the invoice.
    /// </summary>
    public IList<InvoiceItemDto> Items { get; set; } = [];
}

// <copyright file="CreateQuoteCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Command to create a draft sales quote.
/// </summary>
public sealed class CreateQuoteCommand : IRequest<QuoteDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;

    /// <summary>
    /// Gets or sets the customer contact identifier.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the quote date.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the validity end date (30 days after the date when empty).
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Gets or sets the currency code (the country default when empty).
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the payment terms.
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Gets or sets the shipping terms.
    /// </summary>
    public string? ShippingTerms { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the quote lines.
    /// </summary>
    public IList<SalesLineDto> Items { get; set; } = [];
}

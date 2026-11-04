// <copyright file="InvoicePaidDomainEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Events;

namespace FatturaFacile.Invoicing.Domain.Events;

/// <summary>
/// Domain event raised when a payment is applied against an invoice.
/// </summary>
/// <param name="InvoiceId">The unique identifier of the invoice.</param>
/// <param name="TenantId">The owning tenant identifier.</param>
/// <param name="AmountPaid">The monetary payment amount received.</param>
/// <param name="PaidAt">The UTC timestamp when payment was recorded.</param>
public sealed record InvoicePaidDomainEvent(
    Guid InvoiceId,
    Guid TenantId,
    decimal AmountPaid,
    DateTimeOffset PaidAt) : DomainEventBase;

// <copyright file="InvoiceCreatedDomainEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Events;

namespace FatturaFacile.Invoicing.Domain.Events;

/// <summary>
/// Domain event raised whenever an invoice is created or its line items recalculated.
/// </summary>
/// <param name="InvoiceId">The unique identifier of the invoice.</param>
/// <param name="TenantId">The owning tenant identifier.</param>
/// <param name="InvoiceNumber">The unique reference number of the invoice.</param>
/// <param name="TotalGross">The total gross amount including taxes and rounding.</param>
/// <param name="Currency">The 3-letter ISO currency code.</param>
public sealed record InvoiceCreatedDomainEvent(
    Guid InvoiceId,
    Guid TenantId,
    string InvoiceNumber,
    decimal TotalGross,
    string Currency) : DomainEventBase;

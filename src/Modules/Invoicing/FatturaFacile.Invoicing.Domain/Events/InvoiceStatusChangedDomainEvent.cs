// <copyright file="InvoiceStatusChangedDomainEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Events;

namespace FatturaFacile.Invoicing.Domain.Events;

/// <summary>
/// Domain event raised when an invoice transitions between lifecycle payment statuses.
/// </summary>
/// <param name="InvoiceId">The unique identifier of the invoice.</param>
/// <param name="OldStatus">The previous status string.</param>
/// <param name="NewStatus">The newly assigned status string.</param>
public sealed record InvoiceStatusChangedDomainEvent(
    Guid   InvoiceId,
    string OldStatus,
    string NewStatus) : DomainEventBase;

using FatturaFacile.Domain.Shared.Events;

namespace FatturaFacile.Invoicing.Domain.Events;

public sealed record InvoiceCreatedDomainEvent(
    Guid InvoiceId,
    Guid TenantId,
    string InvoiceNumber,
    decimal TotalGross,
    string Currency) : DomainEventBase;

public sealed record InvoicePaidDomainEvent(
    Guid InvoiceId,
    Guid TenantId,
    decimal AmountPaid,
    DateTimeOffset PaidAt) : DomainEventBase;

public sealed record InvoiceStatusChangedDomainEvent(
    Guid InvoiceId,
    string OldStatus,
    string NewStatus) : DomainEventBase;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents dashboard kpi dto.
/// </summary>
/// <param name="TotalRevenueEur">The total revenue eur.</param>
/// <param name="TotalRevenueChf">The total revenue chf.</param>
/// <param name="PendingReceivablesEur">The pending receivables eur.</param>
/// <param name="PendingReceivablesChf">The pending receivables chf.</param>
/// <param name="PendingVatLiabilityEur">The pending vat liability eur.</param>
/// <param name="PendingVatLiabilityChf">The pending vat liability chf.</param>
/// <param name="NextTaxDeadline">The next tax deadline.</param>
/// <param name="ActiveClientsCount">The active clients count.</param>
/// <param name="PendingInvoicesCount">The pending invoices count.</param>
/// <param name="StoredTireSetsCount">The stored tire sets count.</param>
public record DashboardKpiDto(
    decimal TotalRevenueEur,
    decimal TotalRevenueChf,
    decimal PendingReceivablesEur,
    decimal PendingReceivablesChf,
    decimal PendingVatLiabilityEur,
    decimal PendingVatLiabilityChf,
    DateTime? NextTaxDeadline,
    int ActiveClientsCount,
    int PendingInvoicesCount,
    int StoredTireSetsCount);

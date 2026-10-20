namespace FatturaFacile.Application.DTOs;

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
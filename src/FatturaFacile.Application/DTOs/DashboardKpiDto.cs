// <copyright file="DashboardKpiDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation aggregating executive cross-border financial and operational dashboard KPIs.
/// </summary>
/// <param name="TotalRevenueEur">The total gross sales revenue realized in Euro currency.</param>
/// <param name="TotalRevenueChf">The total gross sales revenue realized in Swiss Franc currency.</param>
/// <param name="PendingReceivablesEur">The outstanding accounts receivable balance awaiting settlement in Euros.</param>
/// <param name="PendingReceivablesChf">The outstanding accounts receivable balance awaiting settlement in Swiss Francs.</param>
/// <param name="PendingVatLiabilityEur">The estimated accrued VAT tax liability due for Italian tax authorities.</param>
/// <param name="PendingVatLiabilityChf">The estimated accrued VAT tax liability due for the Swiss Federal Tax Administration.</param>
/// <param name="NextTaxDeadline">The nearest upcoming statutory tax deadline or declaration date, if scheduled.</param>
/// <param name="ActiveClientsCount">The number of currently active client customer accounts.</param>
/// <param name="PendingInvoicesCount">The number of invoices currently awaiting transmission, delivery, or clearance.</param>
/// <param name="StoredTireSetsCount">The total number of seasonal customer tire sets deposited in the warehouse hotel.</param>
public record DashboardKpiDto(
    decimal   TotalRevenueEur,
    decimal   TotalRevenueChf,
    decimal   PendingReceivablesEur,
    decimal   PendingReceivablesChf,
    decimal   PendingVatLiabilityEur,
    decimal   PendingVatLiabilityChf,
    DateTime? NextTaxDeadline,
    int       ActiveClientsCount,
    int       PendingInvoicesCount,
    int       StoredTireSetsCount);

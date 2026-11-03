// <copyright file="ExecutiveKpiSummaryData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

/// <summary>
/// Data transfer object holding aggregated executive financial and tax indicators for a reporting period.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="Period">The reporting period key.</param>
/// <param name="RevenueEur">The total invoiced revenue in Euros.</param>
/// <param name="RevenueChf">The total invoiced revenue in Swiss Francs.</param>
/// <param name="ReceivablesEur">The outstanding accounts receivable in Euros.</param>
/// <param name="ReceivablesChf">The outstanding accounts receivable in Swiss Francs.</param>
/// <param name="VatEur">The estimated accrued Italian VAT liability.</param>
/// <param name="VatChf">The estimated accrued Swiss VAT liability.</param>
/// <param name="NextTaxDeadline">The next upcoming statutory fiscal deadline date.</param>
/// <param name="Jurisdiction">The primary reporting jurisdiction.</param>
public sealed record ExecutiveKpiSummaryData(
    Guid         TenantId,
    string       Period,
    Money        RevenueEur,
    Money        RevenueChf,
    Money        ReceivablesEur,
    Money        ReceivablesChf,
    Money        VatEur,
    Money        VatChf,
    DateTime?    NextTaxDeadline,
    Jurisdiction Jurisdiction);

// <copyright file="VatSettlementPeriodData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents the data structure for a periodic VAT settlement and liquidation, encapsulating key financial metrics such as output VAT, input deductible VAT, and prior credit carried forward.
/// </summary>
/// <param name="TenantId">The unique identifier of the tenant.</param>
/// <param name="Year">The fiscal year of the settlement period.</param>
/// <param name="PeriodNumber">The sequential number of the settlement period within the fiscal year.</param>
/// <param name="PeriodType">The type of the VAT settlement period.</param>
/// <param name="Jurisdiction">The tax jurisdiction applicable to the settlement period.</param>
/// <param name="TotalOutputVat">The total output VAT for the settlement period.</param>
/// <param name="TotalInputVat">The total input VAT for the settlement period.</param>
/// <param name="PriorCreditCarriedForward">The prior credit carried forward to the settlement period.</param>
public sealed record VatSettlementPeriodData(
    Guid                    TenantId,
    int                     Year,
    int                     PeriodNumber,
    VatSettlementPeriodType PeriodType,
    Jurisdiction            Jurisdiction,
    decimal                 TotalOutputVat,
    decimal                 TotalInputVat,
    decimal                 PriorCreditCarriedForward = 0m);

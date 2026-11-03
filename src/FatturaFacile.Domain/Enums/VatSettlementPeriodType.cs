// <copyright file="VatSettlementPeriodType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the statutory reporting cadence for periodic Value Added Tax (VAT / IVA / MWST) calculation and tax settlement.
/// </summary>
public enum VatSettlementPeriodType
{

    /// <summary>
    /// Monthly VAT settlement cadence (Liquidazione IVA mensile), standard for businesses exceeding statutory turnover thresholds.
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Quarterly VAT settlement cadence (Liquidazione IVA trimestrale / Swiss quarterly effective reporting method), applicable to eligible small and mid-sized enterprises.
    /// </summary>
    Quarterly = 2,

}

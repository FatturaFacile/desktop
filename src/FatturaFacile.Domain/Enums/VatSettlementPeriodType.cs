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
    /// Monthly VAT settlement cadence (<c>Liquidazione IVA mensile</c>), standard for businesses exceeding statutory turnover thresholds.
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Quarterly VAT settlement cadence (<c>Liquidazione IVA trimestrale</c> / Swiss quarterly effective reporting method), applicable to eligible small and mid-sized enterprises.
    /// </summary>
    Quarterly = 2,
}

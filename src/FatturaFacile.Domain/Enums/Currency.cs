// <copyright file="Currency.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Defines the supported ISO 4217 standard currency codes for monetary transactions, invoicing, and multi-currency accounting.
/// </summary>
public enum Currency
{

    /// <summary>
    /// Euro (EUR), the official currency of the Eurozone member states and the primary legal tender in Italy.
    /// </summary>
    EUR = 1,

    /// <summary>
    /// Swiss Franc (CHF), the legal tender of Switzerland and Liechtenstein, utilized for domestic Swiss and cross-border billing.
    /// </summary>
    CHF = 2,

    /// <summary>
    /// United States Dollar (USD), the official currency of the United States, used in international trade and foreign exchange invoicing.
    /// </summary>
    USD = 3,

    /// <summary>
    /// British Pound Sterling (GBP), the official currency of the United Kingdom and its territories.
    /// </summary>
    GBP = 4,

}

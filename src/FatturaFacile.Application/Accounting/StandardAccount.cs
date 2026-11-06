// <copyright file="StandardAccount.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// An account used by the automatic postings, created for the company when missing (codes of the Swiss SME chart of
/// accounts, also used as reference for the Italian companies).
/// </summary>
/// <param name="Code">The account code.</param>
/// <param name="Name">The account name.</param>
/// <param name="AccountType">asset, liability, equity, revenue or expense.</param>
/// <param name="NormalBalance">debit or credit.</param>
public sealed record StandardAccount(string Code, string Name, string AccountType, string NormalBalance)
{
    /// <summary>
    /// Gets the bank account.
    /// </summary>
    public static StandardAccount Bank { get; } = new("1020", "Banca c/c / Bankguthaben", "asset", "debit");

    /// <summary>
    /// Gets the trade receivables account.
    /// </summary>
    public static StandardAccount Receivables { get; } = new("1100", "Crediti verso clienti / Forderungen", "asset", "debit");

    /// <summary>
    /// Gets the input VAT account.
    /// </summary>
    public static StandardAccount InputVat { get; } = new("1170", "IVA precedente / Vorsteuer", "asset", "debit");

    /// <summary>
    /// Gets the default fixed asset account.
    /// </summary>
    public static StandardAccount FixedAssets { get; } = new("1500", "Macchinari e impianti / Maschinen und Apparate", "asset", "debit");

    /// <summary>
    /// Gets the accumulated depreciation account.
    /// </summary>
    public static StandardAccount AccumulatedDepreciation { get; } = new("1509", "Fondo ammortamento / Wertberichtigungen", "asset", "credit");

    /// <summary>
    /// Gets the trade payables account.
    /// </summary>
    public static StandardAccount Payables { get; } = new("2000", "Debiti verso fornitori / Verbindlichkeiten", "liability", "credit");

    /// <summary>
    /// Gets the VAT payable account.
    /// </summary>
    public static StandardAccount OutputVat { get; } = new("2200", "IVA dovuta / Geschuldete MWST", "liability", "credit");

    /// <summary>
    /// Gets the default purchase expense account.
    /// </summary>
    public static StandardAccount Purchases { get; } = new("4000", "Acquisti / Materialaufwand", "expense", "debit");

    /// <summary>
    /// Gets the depreciation expense account.
    /// </summary>
    public static StandardAccount Depreciation { get; } = new("6800", "Ammortamenti / Abschreibungen", "expense", "debit");
}

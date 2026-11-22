// <copyright file="LedgerBookCodes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Conversions between a <see cref="LedgerBook"/> and the codes of the web-api (01/02/03, statutory/tax/ifrs).
/// </summary>
public static class LedgerBookCodes
{
    /// <summary>
    /// Gets the report code of a book: 01 statutory, 02 tax, 03 international.
    /// </summary>
    /// <param name="book">The book.</param>
    /// <returns>The two digit code.</returns>
    public static string ToCode(this LedgerBook book) => book switch
    {
        LedgerBook.Statutory => "01",
        LedgerBook.Tax => "02",
        LedgerBook.Ifrs => "03",
        _ => "01",
    };

    /// <summary>
    /// Gets the ledger type name of a book on the wire: statutory, tax or ifrs.
    /// </summary>
    /// <param name="book">The book.</param>
    /// <returns>The ledger type name.</returns>
    public static string ToApiName(this LedgerBook book) => book switch
    {
        LedgerBook.Statutory => "statutory",
        LedgerBook.Tax => "tax",
        LedgerBook.Ifrs => "ifrs",
        _ => "statutory",
    };

    /// <summary>
    /// Reads a ledger type name or report code.
    /// </summary>
    /// <param name="value">The wire value.</param>
    /// <returns>The book; statutory for an unknown value.</returns>
    public static LedgerBook FromApi(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "tax" or "02" => LedgerBook.Tax,
        "ifrs" or "international" or "03" => LedgerBook.Ifrs,
        _ => LedgerBook.Statutory,
    };
}

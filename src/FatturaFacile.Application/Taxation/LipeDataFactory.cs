// <copyright file="LipeDataFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// Builds the data of a LIPE communication from a VAT return and the company that sends it.
/// </summary>
public static class LipeDataFactory
{
    private const string ItalianPrefix = "IT";

    /// <summary>
    /// Builds the data. Nothing is thrown for a malformed identifier: <see cref="LipeRules.Check"/> reports it.
    /// </summary>
    /// <param name="vatReturn">The VAT return of the period.</param>
    /// <param name="tenant">The company.</param>
    /// <param name="passiveOperations">The taxable purchases of the period.</param>
    /// <returns>The data to send.</returns>
    public static LipeData Create(VatReturn vatReturn, Tenant tenant, decimal passiveOperations)
    {
        ArgumentNullException.ThrowIfNull(vatReturn);
        ArgumentNullException.ThrowIfNull(tenant);

        var vatNumber = Normalize(tenant.VatNumber);
        if (vatNumber.StartsWith(ItalianPrefix, StringComparison.Ordinal))
        {
            vatNumber = vatNumber[ItalianPrefix.Length..];
        }

        var taxCode = string.IsNullOrWhiteSpace(tenant.FiscalCode) ? vatNumber : Normalize(tenant.FiscalCode);
        var from = vatReturn.PeriodFrom.Date;
        var monthly = ItalianVatPeriod.IsMonthly(from, vatReturn.PeriodTo.Date);
        var declarant = string.IsNullOrWhiteSpace(tenant.DeclarantFiscalCode) ? null : Normalize(tenant.DeclarantFiscalCode);

        return new LipeData
        {
            TaxCode = taxCode,
            VatNumber = vatNumber,
            Year = from.Year,
            Month = monthly ? from.Month : null,
            Quarter = monthly ? null : ItalianVatPeriod.Quarter(from),
            DeclarantTaxCode = string.Equals(declarant, taxCode, StringComparison.Ordinal) ? null : declarant,
            DeclarantRoleCode = string.IsNullOrWhiteSpace(tenant.DeclarantRoleCode) ? "1" : tenant.DeclarantRoleCode,
            ActiveOperations = vatReturn.TaxableTurnover,
            PassiveOperations = passiveOperations,
            OutputVat = vatReturn.OutputVat,
            InputVat = vatReturn.InputVat,
            PreviousCredit = vatReturn.PreviousCredit,
            Interest = vatReturn.Interest,
            Payable = vatReturn.Payable,
        };
    }

    private static string Normalize(string? value) => string.Concat((value ?? string.Empty).Where(char.IsAsciiLetterOrDigit)).ToUpperInvariant();
}

// <copyright file="ItalianWageCodes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Payroll.Italian;

/// <summary>
/// Codes of the Italian payslip lines.
/// </summary>
public static class ItalianWageCodes
{
    /// <summary>Monthly salary.</summary>
    public const string Salary = "IT1000";

    /// <summary>Thirteenth installment.</summary>
    public const string Thirteenth = "IT1013";

    /// <summary>Fourteenth installment.</summary>
    public const string Fourteenth = "IT1014";

    /// <summary>Unpaid absence.</summary>
    public const string UnpaidAbsence = "IT1900";

    /// <summary>Employee INPS contribution.</summary>
    public const string InpsEmployee = "IT5000";

    /// <summary>IRPEF withholding.</summary>
    public const string Irpef = "IT6000";

    /// <summary>Regional IRPEF surcharge (previous year, 11 installments).</summary>
    public const string RegionalSurcharge = "IT6100";

    /// <summary>Municipal IRPEF surcharge (previous year balance, 11 installments).</summary>
    public const string MunicipalSurcharge = "IT6200";

    /// <summary>Tax wedge amount not subject to tax (cuneo fiscale).</summary>
    public const string TaxWedge = "IT7000";

    /// <summary>Employer INPS contribution.</summary>
    public const string InpsEmployer = "IT8000";

    /// <summary>Severance pay accrued in the month (TFR).</summary>
    public const string Tfr = "IT8100";
}

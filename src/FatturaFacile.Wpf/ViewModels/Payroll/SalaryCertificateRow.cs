// <copyright file="SalaryCertificateRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// A salary certificate shown in the grid with the employee name.
/// </summary>
/// <param name="Certificate">The certificate.</param>
/// <param name="EmployeeName">The employee name.</param>
internal sealed record SalaryCertificateRow(SalaryCertificate Certificate, string EmployeeName)
{
    /// <summary>
    /// Gets the gross salary (box 8).
    /// </summary>
    public decimal Gross => Certificate.Box8Gross;

    /// <summary>
    /// Gets the net salary (box 11).
    /// </summary>
    public decimal Net => Certificate.Box11Net;

    /// <summary>
    /// Gets the social contributions (box 9).
    /// </summary>
    public decimal Contributions => Certificate.Box9SocialContributions;

    /// <summary>
    /// Gets the pension contributions (box 10.1).
    /// </summary>
    public decimal Pension => Certificate.Box10Pension;

    /// <summary>
    /// Gets the withholding tax (box 12).
    /// </summary>
    public decimal WithholdingTax => Certificate.Box12WithholdingTax;

    /// <summary>
    /// Gets the certified period start.
    /// </summary>
    public DateTime PeriodFrom => Certificate.PeriodFrom;

    /// <summary>
    /// Gets the certified period end.
    /// </summary>
    public DateTime PeriodTo => Certificate.PeriodTo;
}

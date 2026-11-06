// <copyright file="PayrollInputRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// A monthly input shown in the grid, with employee and wage type names.
/// </summary>
/// <param name="Input">The input.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="WageTypeName">The wage type description.</param>
internal sealed record PayrollInputRow(PayrollInput Input, string EmployeeName, string WageTypeName)
{
    /// <summary>
    /// Gets the wage type code.
    /// </summary>
    public string Code => Input.WageTypeCode;

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal Quantity => Input.Quantity;

    /// <summary>
    /// Gets the rate.
    /// </summary>
    public decimal Rate => Input.Rate;

    /// <summary>
    /// Gets the amount.
    /// </summary>
    public decimal Amount => Input.Quantity != 0m && Input.Rate != 0m ? Input.Quantity * Input.Rate : Input.Amount;

    /// <summary>
    /// Gets the note.
    /// </summary>
    public string? Note => Input.Note;
}

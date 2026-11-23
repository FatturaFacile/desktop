// <copyright file="LipeRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A VAT liquidation with its LIPE communication.
/// </summary>
/// <param name="ReturnId">The return id.</param>
/// <param name="SubmissionId">The submission id.</param>
/// <param name="Period">The period.</param>
/// <param name="Output">The output.</param>
/// <param name="Input">The input.</param>
/// <param name="Payable">The payable.</param>
/// <param name="Status">The status.</param>
/// <param name="Detail">The detail.</param>
internal sealed record LipeRow(Guid ReturnId, Guid? SubmissionId, string Period, string Output, string Input, string Payable, string Status, string Detail);

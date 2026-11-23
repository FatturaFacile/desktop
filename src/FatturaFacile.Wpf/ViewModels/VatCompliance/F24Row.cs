// <copyright file="F24Row.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A Modello F24 payment of the withholdings.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Period">The period.</param>
/// <param name="TaxCode">The tax code.</param>
/// <param name="Amount">The amount.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="Paid">The paid.</param>
/// <param name="Reference">The reference.</param>
/// <param name="State">The state.</param>
internal sealed record F24Row(Guid Id, string Period, string TaxCode, string Amount, string DueDate, string Paid, string Reference, string State);

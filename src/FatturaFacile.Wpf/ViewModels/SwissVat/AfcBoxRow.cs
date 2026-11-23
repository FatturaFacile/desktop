// <copyright file="AfcBoxRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.SwissVat;

/// <summary>
/// Display row for an official AFC / ESTV Form 050/051 box line.
/// </summary>
/// <param name="Code">Official box code.</param>
/// <param name="Section">Form section.</param>
/// <param name="Base">Taxable base formatted string.</param>
/// <param name="Rate">Rate percentage formatted string.</param>
/// <param name="Amount">Amount in CHF formatted string.</param>
internal sealed record AfcBoxRow(string Code, string Section, string Base, string Rate, string Amount);

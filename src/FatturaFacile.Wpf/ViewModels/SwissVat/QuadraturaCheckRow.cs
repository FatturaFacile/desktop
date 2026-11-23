// <copyright file="QuadraturaCheckRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.SwissVat;

/// <summary>
/// Display row for an accounting quadratura check.
/// </summary>
/// <param name="Code">Check identifier.</param>
/// <param name="Severity">Blocker or warning.</param>
/// <param name="Status">Pass/fail status label.</param>
/// <param name="Expected">Expected value.</param>
/// <param name="Actual">Actual value.</param>
/// <param name="Difference">Absolute difference.</param>
/// <param name="Message">Localized diagnostic description.</param>
internal sealed record QuadraturaCheckRow(
    string Code,
    string Severity,
    string Status,
    string Expected,
    string Actual,
    string Difference,
    string Message);

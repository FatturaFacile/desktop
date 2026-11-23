// <copyright file="BlanketAlertRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One alert of a blanket agreement balance, formatted for display.
/// </summary>
/// <param name="Code">The alert code.</param>
/// <param name="Text">The localized alert.</param>
/// <param name="Severity">The severity code (warning or critical), used by the view to colour the alert.</param>
/// <param name="SeverityText">The localized severity.</param>
internal sealed record BlanketAlertRow(string Code, string Text, string Severity, string SeverityText);

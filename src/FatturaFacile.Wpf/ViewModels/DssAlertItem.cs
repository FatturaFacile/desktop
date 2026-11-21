// <copyright file="DssAlertItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A strategic alert as the cockpit lists it.
/// </summary>
/// <param name="Severity">CRITICAL, WARNING or INFO.</param>
/// <param name="SeverityText">The localized severity.</param>
/// <param name="Title">The localized alert title.</param>
/// <param name="Detail">The offending value against the threshold, and the customer when named.</param>
internal sealed record DssAlertItem(string Severity, string SeverityText, string Title, string Detail);

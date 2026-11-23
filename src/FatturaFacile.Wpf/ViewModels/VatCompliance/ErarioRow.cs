// <copyright file="ErarioRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A period of the Erario c/IVA reconciliation.
/// </summary>
/// <param name="Period">The period.</param>
/// <param name="LocalBalance">The local balance.</param>
/// <param name="Server">The server.</param>
/// <param name="Ledger">The ledger.</param>
/// <param name="Difference">The difference.</param>
/// <param name="State">The state.</param>
internal sealed record ErarioRow(string Period, string LocalBalance, string Server, string Ledger, string Difference, string State);

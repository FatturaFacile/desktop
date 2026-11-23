// <copyright file="IntentLetterRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A declaration of intent in the plafond grid.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Protocol">The protocol.</param>
/// <param name="Contact">The contact.</param>
/// <param name="Receipt">The receipt.</param>
/// <param name="Validity">The validity.</param>
/// <param name="Plafond">The plafond.</param>
/// <param name="Consumed">The consumed.</param>
/// <param name="Residual">The residual.</param>
/// <param name="State">The state.</param>
internal sealed record IntentLetterRow(Guid Id, string Protocol, string Contact, string Receipt, string Validity, string Plafond, string Consumed, string Residual, string State);

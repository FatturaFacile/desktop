// <copyright file="ReceivablesOverrideRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the credit overrides.
/// </summary>
/// <param name="Override">The override.</param>
/// <param name="Summary">The scope and the reason.</param>
/// <param name="Detail">Who granted it and until when.</param>
/// <param name="CanRevoke">Whether the override is still in force.</param>
internal sealed record ReceivablesOverrideRow(CreditOverride Override, string Summary, string Detail, bool CanRevoke);

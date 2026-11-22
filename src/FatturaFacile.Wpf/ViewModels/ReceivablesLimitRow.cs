// <copyright file="ReceivablesLimitRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the credit limit history.
/// </summary>
/// <param name="Limit">The limit.</param>
/// <param name="Summary">The amount and the validity.</param>
/// <param name="Detail">The policy, the insurance and the tolerance.</param>
/// <param name="IsCurrent">Whether the limit is in force today.</param>
internal sealed record ReceivablesLimitRow(CustomerCreditLimit Limit, string Summary, string Detail, bool IsCurrent);

// <copyright file="ReceivablesBlockedRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A customer blocked for credit.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Name">The customer name.</param>
/// <param name="Reason">The reason.</param>
/// <param name="ExposureText">The exposure against the limit.</param>
internal sealed record ReceivablesBlockedRow(Guid CustomerId, string Name, string Reason, string ExposureText);

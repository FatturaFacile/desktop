// <copyright file="HelpdeskContractRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A contract of the customer 360 panel.
/// </summary>
/// <param name="Name">The name of the contract.</param>
/// <param name="BankText">The hour bank, or a note that there is none.</param>
/// <param name="IsLow">Whether the hour bank is used up.</param>
internal sealed record HelpdeskContractRow(string Name, string BankText, bool IsLow);

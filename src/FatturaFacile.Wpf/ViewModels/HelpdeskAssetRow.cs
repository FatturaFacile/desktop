// <copyright file="HelpdeskAssetRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An asset of the customer 360 panel.
/// </summary>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="WarrantyText">The warranty note.</param>
/// <param name="IsExpired">Whether the warranty has ended.</param>
internal sealed record HelpdeskAssetRow(string SerialNumber, string WarrantyText, bool IsExpired);

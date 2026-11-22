// <copyright file="FieldServiceEquipmentRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A customer asset of the picker, formatted for display.
/// </summary>
/// <param name="Id">The equipment identifier.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="SiteText">The site address.</param>
/// <param name="MetersText">The operating hours and cycles.</param>
/// <param name="NextServiceText">The next scheduled service date.</param>
internal sealed record FieldServiceEquipmentRow(string Id, string SerialNumber, string SiteText, string MetersText, string NextServiceText);

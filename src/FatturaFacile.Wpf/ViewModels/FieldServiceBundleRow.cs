// <copyright file="FieldServiceBundleRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A work order of the offline bundle of a technician, formatted for display.
/// </summary>
/// <param name="Id">The work order identifier.</param>
/// <param name="Number">The number.</param>
/// <param name="Title">The title.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="WindowText">The scheduled window.</param>
/// <param name="CustomerText">The customer name.</param>
/// <param name="EquipmentText">The serial number of the equipment.</param>
/// <param name="ChecklistText">The checklist template.</param>
internal sealed record FieldServiceBundleRow(string Id, string Number, string Title, string StatusText, string WindowText, string CustomerText, string EquipmentText, string ChecklistText);

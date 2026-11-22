// <copyright file="FieldServiceUnassignedRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A work order of the unassigned queue, formatted for display.
/// </summary>
/// <param name="WorkOrderId">The work order identifier.</param>
/// <param name="Number">The work order number.</param>
/// <param name="Title">The title.</param>
/// <param name="TypeText">The localized work type.</param>
/// <param name="PriorityText">The localized priority.</param>
/// <param name="Priority">The raw priority, for the colour.</param>
/// <param name="SkillsText">The required skills.</param>
/// <param name="SiteText">The site address and coordinates.</param>
/// <param name="PartsText">The number of required parts.</param>
internal sealed record FieldServiceUnassignedRow(string WorkOrderId, string Number, string Title, string TypeText, string PriorityText, string Priority, string SkillsText, string SiteText, string PartsText);

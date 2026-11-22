// <copyright file="FieldServiceChecklistItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// One line of the checklist of a service report.
/// </summary>
/// <param name="Item">The checklist item.</param>
/// <param name="Result">The result: pass, fail or na.</param>
/// <param name="Notes">The notes.</param>
public sealed record FieldServiceChecklistItem(string Item, string Result, string? Notes);

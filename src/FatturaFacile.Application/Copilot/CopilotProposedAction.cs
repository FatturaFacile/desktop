// <copyright file="CopilotProposedAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A write operation the assistant proposes and that runs only after the user confirms it.
/// </summary>
/// <param name="Tool">The tool that executes the action.</param>
/// <param name="Module">The ERP module the action belongs to.</param>
/// <param name="Status">The status reported with the proposal.</param>
/// <param name="Summary">The human readable summary.</param>
/// <param name="Arguments">The flattened arguments.</param>
/// <param name="Details">The facts shown on the confirmation card.</param>
/// <param name="EditableFields">The dotted argument paths the user may change before confirming.</param>
/// <param name="Result">The result text once executed.</param>
/// <param name="Error">The error text when the action failed.</param>
public sealed record CopilotProposedAction(
    string Tool,
    string Module,
    string Status,
    string Summary,
    IReadOnlyList<CopilotArgument> Arguments,
    IReadOnlyDictionary<string, string> Details,
    IReadOnlyList<string> EditableFields,
    string? Result,
    string? Error);

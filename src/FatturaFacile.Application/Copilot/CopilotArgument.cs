// <copyright file="CopilotArgument.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// One leaf of the arguments of a proposed action, addressed by its dotted path (e.g. "items.0.quantity").
/// </summary>
/// <param name="Path">The dotted path.</param>
/// <param name="Value">The current value as text.</param>
/// <param name="Kind">The JSON kind of the value, used to convert an override back.</param>
public sealed record CopilotArgument(string Path, string Value, CopilotArgumentKind Kind);

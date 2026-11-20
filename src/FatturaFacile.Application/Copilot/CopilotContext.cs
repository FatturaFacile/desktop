// <copyright file="CopilotContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// The place of the application the user is looking at, sent with each prompt so the assistant can resolve "this invoice".
/// </summary>
/// <param name="Route">The route (module) of the active view, e.g. "invoices".</param>
/// <param name="EntityType">The type of the selected entity, e.g. "invoice", or null.</param>
/// <param name="EntityId">The identifier of the selected entity, or null.</param>
public sealed record CopilotContext(string? Route, string? EntityType, string? EntityId)
{
    /// <summary>
    /// Gets an empty context.
    /// </summary>
    public static CopilotContext Empty { get; } = new(Route: null, EntityType: null, EntityId: null);
}

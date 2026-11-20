// <copyright file="CopilotCitation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A source cited by the assistant: either a regulatory snippet or a link to an ERP entity.
/// </summary>
/// <param name="Type">"regulatory" or "entity".</param>
/// <param name="Label">The short label, e.g. "[LIVA Art. 25]" or "#Invoice-2026-0042".</param>
/// <param name="Title">The title of the regulatory source.</param>
/// <param name="SectionRef">The section reference of the regulatory source.</param>
/// <param name="Jurisdiction">The jurisdiction (CH, IT) of the regulatory source.</param>
/// <param name="EntityType">The entity type of an entity citation.</param>
/// <param name="EntityId">The entity identifier of an entity citation.</param>
/// <param name="Route">The route of an entity citation.</param>
public sealed record CopilotCitation(
    string Type,
    string Label,
    string? Title,
    string? SectionRef,
    string? Jurisdiction,
    string? EntityType,
    string? EntityId,
    string? Route)
{
    /// <summary>
    /// Gets a value indicating whether the citation links to an ERP entity.
    /// </summary>
    public bool IsEntity => string.Equals(Type, "entity", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the citation is a regulatory snippet.
    /// </summary>
    public bool IsRegulatory => !IsEntity;
}

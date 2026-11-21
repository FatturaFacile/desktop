// <copyright file="PiiUnmaskRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// A request to reveal a masked personal-data field.
/// </summary>
/// <param name="EntityType">The entity type.</param>
/// <param name="EntityId">The entity identifier.</param>
/// <param name="Field">The masked field name.</param>
/// <param name="Justification">The reason (at least <see cref="PiiUnmaskRules.MinimumJustificationLength"/> characters).</param>
public sealed record PiiUnmaskRequest(string EntityType, string EntityId, string Field, string Justification);

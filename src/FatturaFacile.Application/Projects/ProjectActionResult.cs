// <copyright file="ProjectActionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The answer to a project change.
/// </summary>
/// <param name="Status">What happened.</param>
/// <param name="Detail">The localization key of the validation error for a rejection, or the error of the web-api for a conflict.</param>
/// <param name="EntityId">The identifier of the record that was created or changed, when there is one.</param>
public sealed record ProjectActionResult(ProjectActionStatus Status, string? Detail = null, Guid? EntityId = null);

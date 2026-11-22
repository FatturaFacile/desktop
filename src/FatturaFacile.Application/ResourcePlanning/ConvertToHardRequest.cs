// <copyright file="ConvertToHardRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that converts a soft booking into a hard commitment.
/// </summary>
/// <param name="ProjectId">The project, required when the soft booking only has an opportunity.</param>
/// <param name="WbsNodeId">The WBS node of the project, or null.</param>
public sealed record ConvertToHardRequest(
    Guid? ProjectId,
    Guid? WbsNodeId);

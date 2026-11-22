// <copyright file="ProjectEvmNode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// A node of the WBS cost breakdown.
/// </summary>
/// <param name="Id">The WBS node identifier.</param>
/// <param name="Code">The WBS code.</param>
/// <param name="Title">The title.</param>
/// <param name="Figures">The figures, or null until a snapshot exists for the node.</param>
/// <param name="Children">The child nodes.</param>
public sealed record ProjectEvmNode(string Id, string Code, string Title, ProjectEvmFigures? Figures, IReadOnlyList<ProjectEvmNode> Children);

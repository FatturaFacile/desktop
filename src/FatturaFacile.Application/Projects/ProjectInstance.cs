// <copyright file="ProjectInstance.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// A project built from a template: new identifiers everywhere, dates shifted, actuals and schedule results cleared.
/// </summary>
/// <param name="Project">The new project.</param>
/// <param name="Nodes">The copied nodes.</param>
/// <param name="Dependencies">The copied dependencies, pointing at the copied nodes.</param>
public sealed record ProjectInstance(Project Project, IReadOnlyList<WbsNode> Nodes, IReadOnlyList<TaskDependency> Dependencies);

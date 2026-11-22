// <copyright file="WbsTreeItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// A node at its place in the flattened WBS tree.
/// </summary>
/// <param name="Node">The node.</param>
/// <param name="Depth">The nesting level, 0 for a top-level node.</param>
/// <param name="HasChildren">Whether the node is a summary node.</param>
public sealed record WbsTreeItem(WbsNode Node, int Depth, bool HasChildren);

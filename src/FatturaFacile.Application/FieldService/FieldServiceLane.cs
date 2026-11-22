// <copyright file="FieldServiceLane.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The lane of a technician on the dispatcher board.
/// </summary>
/// <param name="TechnicianId">The technician identifier.</param>
/// <param name="Name">The technician name.</param>
/// <param name="IsAvailable">Whether the technician is available.</param>
/// <param name="Skills">The normalized skills of the technician.</param>
/// <param name="VanReady">Whether the technician has a van warehouse.</param>
/// <param name="VanProducts">The distinct products in stock on the van.</param>
/// <param name="Blocks">The scheduled work orders of the window.</param>
public sealed record FieldServiceLane(string TechnicianId, string Name, bool IsAvailable, IReadOnlyList<string> Skills, bool VanReady, int VanProducts, IReadOnlyList<FieldServiceBlock> Blocks);

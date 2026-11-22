// <copyright file="FieldServicePreventiveResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The outcome of a run of the preventive maintenance engine.
/// </summary>
/// <param name="Status">The outcome.</param>
/// <param name="Evaluated">The assets the engine evaluated.</param>
/// <param name="Created">The work orders created.</param>
/// <param name="Skipped">The cycles that already had a work order.</param>
/// <param name="Detail">The error detail, when there is one.</param>
public sealed record FieldServicePreventiveResult(FieldServiceCallStatus Status, int Evaluated, int Created, int Skipped, string? Detail);

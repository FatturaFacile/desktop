// <copyright file="FieldServiceDispatchResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// How the dispatch of a work order to a technician ended.
/// </summary>
/// <param name="Status">The outcome; Rejected when the server blocked the dispatch.</param>
/// <param name="Warnings">The blockers the server reported (schedule, skills, van stock).</param>
/// <param name="Detail">The error detail, when there is one.</param>
public sealed record FieldServiceDispatchResult(FieldServiceCallStatus Status, IReadOnlyList<FieldServiceWarning> Warnings, string? Detail);

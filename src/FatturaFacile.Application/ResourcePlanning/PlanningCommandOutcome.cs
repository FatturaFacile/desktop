// <copyright file="PlanningCommandOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The answer of a write on the resource planning API: the value when it succeeded, the violations of the server when it refused.
/// </summary>
/// <typeparam name="T">The kind of value.</typeparam>
/// <param name="Status">The HTTP status.</param>
/// <param name="Value">The value of a successful write, otherwise null.</param>
/// <param name="Issues">The violations of a refused write (the invalid_params of the problem document).</param>
public sealed record PlanningCommandOutcome<T>(HttpStatusCode Status, T? Value, IReadOnlyList<PlanningIssue> Issues)
{
    /// <summary>
    /// Gets a value indicating whether the write was accepted (2xx).
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}

// <copyright file="PlanningDraftResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The result of checking what the planner entered before it is sent: the request to send, or the code of the first problem.
/// </summary>
/// <typeparam name="T">The request that is built.</typeparam>
/// <param name="Request">The request, or null when the draft is not valid.</param>
/// <param name="ErrorCode">The code of the problem (a suffix of the localization key), or null when the draft is valid.</param>
public sealed record PlanningDraftResult<T>(T? Request, string? ErrorCode)
    where T : class
{
    /// <summary>
    /// Gets a value indicating whether the draft is valid.
    /// </summary>
    public bool IsValid => Request is not null;

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The result.</returns>
    public static PlanningDraftResult<T> Ok(T request) => new(request, null);

    /// <summary>
    /// Creates a result that carries a problem.
    /// </summary>
    /// <param name="code">The code of the problem.</param>
    /// <returns>The result.</returns>
    public static PlanningDraftResult<T> Fail(string code) => new(null, code);
}

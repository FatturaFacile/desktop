// <copyright file="PlanningDraftResults.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Creates the <see cref="PlanningDraftResult{T}"/> of a draft check.
/// </summary>
public static class PlanningDraftResults
{
    /// <summary>
    /// Creates a valid result.
    /// </summary>
    /// <typeparam name="T">The request that is built.</typeparam>
    /// <param name="request">The request.</param>
    /// <returns>The result.</returns>
    public static PlanningDraftResult<T> Ok<T>(T request)
        where T : class => new(request, ErrorCode: null);

    /// <summary>
    /// Creates a result that carries a problem.
    /// </summary>
    /// <typeparam name="T">The request that is built.</typeparam>
    /// <param name="code">The code of the problem.</param>
    /// <returns>The result.</returns>
    public static PlanningDraftResult<T> Fail<T>(string code)
        where T : class => new(Request: null, code);
}

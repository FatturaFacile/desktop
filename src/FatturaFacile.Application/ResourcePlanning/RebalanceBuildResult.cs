// <copyright file="RebalanceBuildResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The result of checking a rebalance draft against its allocation: the request to send, or the code of the first problem.
/// </summary>
/// <param name="Request">The request, or null when the draft is not valid.</param>
/// <param name="ErrorCode">The code of the problem (a suffix of the localization key), or null when the draft is valid.</param>
public sealed record RebalanceBuildResult(RebalanceRequest? Request, string? ErrorCode)
{
    /// <summary>
    /// Gets a value indicating whether the draft is valid.
    /// </summary>
    public bool IsValid => Request is not null;
}

// <copyright file="ApiProblemEventArgs.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Event data of a refusal of the API.
/// </summary>
/// <param name="kind">The refusal.</param>
public sealed class ApiProblemEventArgs(ApiProblemKind kind) : EventArgs
{
    /// <summary>
    /// Gets the refusal.
    /// </summary>
    public ApiProblemKind Kind { get; } = kind;
}

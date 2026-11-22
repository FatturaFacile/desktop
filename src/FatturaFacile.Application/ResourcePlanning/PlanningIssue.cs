// <copyright file="PlanningIssue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A machine readable warning or violation of the server with its parameters.
/// </summary>
/// <param name="Code">The code, for example overload or hard_booking_requires_project.</param>
/// <param name="Params">The parameters of the message, or null.</param>
public sealed record PlanningIssue(
    string Code,
    JsonElement? Params);

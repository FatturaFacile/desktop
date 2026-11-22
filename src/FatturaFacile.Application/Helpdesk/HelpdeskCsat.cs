// <copyright file="HelpdeskCsat.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The customer satisfaction figures of a metrics group.
/// </summary>
/// <param name="Responses">The number of survey responses.</param>
/// <param name="Average">The average score, when there are responses.</param>
public sealed record HelpdeskCsat(int Responses, double? Average);

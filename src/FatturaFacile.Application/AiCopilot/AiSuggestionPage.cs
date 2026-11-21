// <copyright file="AiSuggestionPage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// One page of the open suggestion queue.
/// </summary>
/// <param name="Items">The suggestions, most urgent first.</param>
/// <param name="Total">The total number of open suggestions on the server.</param>
public sealed record AiSuggestionPage(IReadOnlyList<AiSuggestionDto> Items, int Total);

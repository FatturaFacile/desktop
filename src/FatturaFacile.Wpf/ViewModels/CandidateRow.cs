// <copyright file="CandidateRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One suggested employee of the candidate search, ready to show.
/// </summary>
/// <param name="Candidate">The candidate.</param>
/// <param name="ScoreText">The fit.</param>
/// <param name="EligibilityText">Whether the candidate qualifies.</param>
/// <param name="MatchedText">The skills that matched.</param>
/// <param name="BlockersText">Why the candidate does not qualify.</param>
/// <param name="AvailabilityText">The available hours and the free hours per day.</param>
/// <param name="PeakAfterText">The highest load after the booking.</param>
/// <param name="OverloadsAfter">Whether the booking would overload the employee.</param>
/// <param name="CostText">The hourly cost.</param>
internal sealed record CandidateRow(ResourceCandidate Candidate, string ScoreText, string EligibilityText, string MatchedText, string BlockersText, string AvailabilityText, string PeakAfterText, bool OverloadsAfter, string CostText);

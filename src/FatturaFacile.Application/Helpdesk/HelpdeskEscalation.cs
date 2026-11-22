// <copyright file="HelpdeskEscalation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// An SLA escalation step that fired on a ticket.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="Target">The SLA target the step belongs to.</param>
/// <param name="ThresholdPercent">The threshold that triggered the step.</param>
/// <param name="ElapsedPercent">The elapsed share when it fired.</param>
/// <param name="Action">The action taken.</param>
/// <param name="Outcome">The outcome.</param>
/// <param name="FiredAt">The instant it fired.</param>
public sealed record HelpdeskEscalation(Guid? Id, string Target, double ThresholdPercent, double ElapsedPercent, string Action, string Outcome, DateTime? FiredAt);

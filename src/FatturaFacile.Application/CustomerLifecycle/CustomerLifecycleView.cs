// <copyright file="CustomerLifecycleView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// The summary and the timeline of a customer.
/// </summary>
/// <param name="Summary">The summary.</param>
/// <param name="Timeline">The timeline, newest first.</param>
public sealed record CustomerLifecycleView(CustomerLifecycleSummary Summary, IReadOnlyList<CustomerTimelineEvent> Timeline);

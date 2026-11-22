// <copyright file="IHelpdeskService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The offline-first helpdesk: reads go to the server and fall back to the cache, writes go through the outbox.
/// </summary>
public interface IHelpdeskService
{
    /// <summary>
    /// Loads the workbench: sends the queued actions first, then pulls the breach-risk list (or reads the cache).
    /// </summary>
    /// <param name="riskPercent">The minimum elapsed share of the tickets to list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The workbench.</returns>
    public Task<HelpdeskWorkbench> LoadWorkbenchAsync(int riskPercent, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the SLA metrics of a period.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="groupBy">The grouping: queue, priority or contract.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The metrics, or null when the server is unreachable and nothing is cached.</returns>
    public Task<HelpdeskLoad<HelpdeskMetrics>?> LoadMetricsAsync(DateOnly from, DateOnly to, string groupBy, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the customer 360 context of a ticket.
    /// </summary>
    /// <param name="ticketId">The ticket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The context, or null when the server is unreachable and nothing is cached.</returns>
    public Task<HelpdeskLoad<HelpdeskCustomerContext>?> LoadCustomerContextAsync(Guid ticketId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the escalation history of a ticket.
    /// </summary>
    /// <param name="ticketId">The ticket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The history, or null when the server is unreachable and nothing is cached.</returns>
    public Task<HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>?> LoadEscalationsAsync(Guid ticketId, CancellationToken cancellationToken);

    /// <summary>
    /// Searches the knowledge base.
    /// </summary>
    /// <param name="text">The text to look for.</param>
    /// <param name="locale">The knowledge base locale, see <see cref="HelpdeskKnowledgeLocale"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The articles, or null when the server is unreachable and the search was never made.</returns>
    public Task<HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>?> SearchKnowledgeAsync(string text, string locale, CancellationToken cancellationToken);

    /// <summary>
    /// Logs time on a ticket: validates, queues and sends when the server is reachable.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<HelpdeskActionResult> LogTimeAsync(HelpdeskTimeLogRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Escalates a ticket to a Field Service work order.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<HelpdeskActionResult> EscalateToFieldServiceAsync(HelpdeskFieldServiceRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Escalates a ticket to an RMA.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<HelpdeskActionResult> EscalateToRmaAsync(HelpdeskRmaRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Sends the queued actions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<HelpdeskFlushResult> FlushAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges an action the server refused so it leaves the list.
    /// </summary>
    /// <param name="entryId">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the entry is dismissed.</returns>
    public Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken);
}

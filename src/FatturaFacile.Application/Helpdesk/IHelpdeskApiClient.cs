// <copyright file="IHelpdeskApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// Port to the helpdesk endpoints of the web-api (/api/v1/esm and the knowledge base).
/// </summary>
public interface IHelpdeskApiClient
{
    /// <summary>
    /// Gets a value indicating whether a session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the tickets at risk of breaching or already breaching their SLA.
    /// </summary>
    /// <param name="riskPercent">The minimum elapsed share.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetBreachRiskAsync(int riskPercent, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the SLA metrics of a period.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="groupBy">The grouping: queue, priority or contract.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetMetricsAsync(DateOnly from, DateOnly to, string groupBy, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the customer 360 context of a ticket.
    /// </summary>
    /// <param name="ticketId">The ticket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetCustomerContextAsync(Guid ticketId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the escalation history of a ticket.
    /// </summary>
    /// <param name="ticketId">The ticket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetEscalationsAsync(Guid ticketId, CancellationToken cancellationToken);

    /// <summary>
    /// Searches the knowledge base.
    /// </summary>
    /// <param name="text">The text to look for.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> SearchKnowledgeAsync(string text, string locale, CancellationToken cancellationToken);

    /// <summary>
    /// Posts an action of a ticket.
    /// </summary>
    /// <param name="route">The route relative to the API root.</param>
    /// <param name="jsonBody">The JSON body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw answer.</returns>
    public Task<HelpdeskApiReply> SendAsync(string route, string jsonBody, CancellationToken cancellationToken);
}

// <copyright file="ICustomerLifecycleApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// Typed client of the web-api customer 360 endpoints. Network and HTTP failures surface as <see cref="HttpRequestException"/>.
/// </summary>
public interface ICustomerLifecycleApiClient
{
    /// <summary>
    /// Finds the CRM account of a contact.
    /// </summary>
    /// <param name="contactId">The contact (party master) identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account identifier, or null when the contact has no account.</returns>
    public Task<Guid?> FindAccountIdAsync(Guid contactId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the 360 summary of an account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary.</returns>
    public Task<CustomerLifecycleSummary> GetSummaryAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the timeline of an account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="types">The touchpoint types to include; empty means all.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The events, newest first.</returns>
    public Task<IReadOnlyList<CustomerTimelineEvent>> GetTimelineAsync(Guid accountId, IReadOnlyCollection<string> types, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a CRM activity (visit or follow-up) related to an account. A network failure surfaces as <see cref="HttpRequestException"/>
    /// without a status code; a refusal of the server carries its status code.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="type">The activity type (see <see cref="CustomerActivityTypes"/>).</param>
    /// <param name="subject">The subject.</param>
    /// <param name="dueDate">The optional due date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the server accepted the activity.</returns>
    public Task CreateActivityAsync(Guid accountId, string type, string subject, DateTime? dueDate, CancellationToken cancellationToken);
}

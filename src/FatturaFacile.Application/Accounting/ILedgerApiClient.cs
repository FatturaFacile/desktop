// <copyright file="ILedgerApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Gateway to the General Ledger endpoints of the web-api (/api/v1/general-ledger). Every method returns the `data` object of the
/// answer as JSON, which <see cref="LedgerApiMapper"/> reads; a refused request raises an <see cref="HttpRequestException"/> with the status.
/// </summary>
public interface ILedgerApiClient
{
    /// <summary>
    /// Gets a value indicating whether a web-api session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Lists the accounting templates (GET accounting-templates).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetTemplatesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the chart of accounts (GET accounts).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetAccountsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Previews a template on the server (POST accounting-templates/{id}/preview).
    /// </summary>
    /// <param name="templateId">The template.</param>
    /// <param name="body">The request JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> PreviewTemplateAsync(Guid templateId, string body, CancellationToken cancellationToken);

    /// <summary>
    /// Posts a template on the server (POST accounting-templates/{id}/post).
    /// </summary>
    /// <param name="templateId">The template.</param>
    /// <param name="body">The request JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> PostTemplateAsync(Guid templateId, string body, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a journal entry (POST journals).
    /// </summary>
    /// <param name="body">The request JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> CreateJournalAsync(string body, CancellationToken cancellationToken);

    /// <summary>
    /// Reverses a journal entry (POST journals/{id}/reverse).
    /// </summary>
    /// <param name="journalId">The server journal.</param>
    /// <param name="body">The request JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> ReverseJournalAsync(Guid journalId, string body, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the server trial balance (GET reports/trial-balance).
    /// </summary>
    /// <param name="query">The query string without the leading question mark.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetTrialBalanceAsync(string query, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the server account ledger (GET reports/account-ledger).
    /// </summary>
    /// <param name="query">The query string without the leading question mark.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data JSON.</returns>
    public Task<string> GetAccountLedgerAsync(string query, CancellationToken cancellationToken);
}

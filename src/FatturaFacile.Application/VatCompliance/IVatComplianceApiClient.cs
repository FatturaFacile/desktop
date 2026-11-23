// <copyright file="IVatComplianceApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Sync;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Access to the VAT compliance endpoints of the web-api: VAT registers, periodic settlements, declarations of intent and
/// withholding settlements. Reads return the `data` element of the answer as JSON; network failures surface as
/// <see cref="HttpRequestException"/>.
/// </summary>
public interface IVatComplianceApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads the declarations of intent of a year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetIntentLettersAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the VAT settlements of a year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetSettlementsAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the withholding settlements of a year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetWithholdingSettlementsAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Reads one withholding settlement with its lines.
    /// </summary>
    /// <param name="settlementId">The settlement identifier on the web-api.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetWithholdingSettlementAsync(Guid settlementId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a VAT register of a period.
    /// </summary>
    /// <param name="type">The register: "sales" or "purchases".</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array of the register lines.</returns>
    public Task<string> GetRegisterAsync(string type, DateTime from, DateTime to, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a declaration of intent.
    /// </summary>
    /// <param name="json">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and body.</returns>
    public Task<SyncHttpReply> CreateIntentLetterAsync(string json, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a declaration of intent.
    /// </summary>
    /// <param name="serverId">The identifier on the web-api.</param>
    /// <param name="json">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and body.</returns>
    public Task<SyncHttpReply> UpdateIntentLetterAsync(Guid serverId, string json, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a declaration of intent.
    /// </summary>
    /// <param name="serverId">The identifier on the web-api.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and body.</returns>
    public Task<SyncHttpReply> DeleteIntentLetterAsync(Guid serverId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a withholding settlement as paid.
    /// </summary>
    /// <param name="serverId">The settlement identifier on the web-api.</param>
    /// <param name="json">The request body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and body.</returns>
    public Task<SyncHttpReply> PayWithholdingSettlementAsync(Guid serverId, string json, CancellationToken cancellationToken);
}

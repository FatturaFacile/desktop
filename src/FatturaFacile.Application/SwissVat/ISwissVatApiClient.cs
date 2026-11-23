// <copyright file="ISwissVatApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Port interface for communicating with the FatturaFacile web-api Swiss VAT endpoints (`/api/v1/swiss-vat/*`).
/// </summary>
public interface ISwissVatApiClient
{
    /// <summary>
    /// Gets a value indicating whether an authenticated session is available.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Fetches the active tenant's Swiss VAT configuration JSON payload.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON payload string.</returns>
    public Task<string> GetConfigAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the active tenant's Swiss VAT configuration.
    /// </summary>
    /// <param name="json">JSON request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string.</returns>
    public Task<string> UpdateConfigAsync(string json, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches periodic Swiss VAT returns for <paramref name="year"/>.
    /// </summary>
    /// <param name="year">Fiscal year.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON payload string.</returns>
    public Task<string> GetReturnsAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Calculates a Swiss VAT periodic return on the server.
    /// </summary>
    /// <param name="json">JSON calculation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string.</returns>
    public Task<string> CalculateReturnAsync(string json, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches Bezugsteuer entries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON payload string.</returns>
    public Task<string> GetBezugsteuerAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Triggers server-side auto-detection of foreign supplier bills subject to Bezugsteuer.
    /// </summary>
    /// <param name="periodFrom">Start date (yyyy-MM-dd).</param>
    /// <param name="periodTo">End date (yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string.</returns>
    public Task<string> DetectBezugsteuerAsync(string periodFrom, string periodTo, CancellationToken cancellationToken);

    /// <summary>
    /// Confirms a detected Bezugsteuer entry.
    /// </summary>
    /// <param name="id">Entry identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string.</returns>
    public Task<string> ConfirmBezugsteuerAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches LIVA Art. 30-33 input tax adjustments.
    /// </summary>
    /// <param name="year">Fiscal year.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON payload string.</returns>
    public Task<string> GetAdjustmentsAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Validates and exports SuisseTax XML for a return.
    /// </summary>
    /// <param name="returnId">Swiss VAT return identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON export metadata response.</returns>
    public Task<string> ExportSuisseTaxXmlAsync(Guid returnId, CancellationToken cancellationToken);
}

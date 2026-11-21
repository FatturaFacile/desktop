// <copyright file="IDssApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Typed client of the web-api Executive DSS endpoints (`/api/v1/analytics/dss`). Every method returns the `data` object of the
/// answer as received. Network and HTTP failures surface as <see cref="HttpRequestException"/> (with the status code when the
/// server answered).
/// </summary>
public interface IDssApiClient
{
    /// <summary>
    /// Gets a value indicating whether a server session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Fetches the overview (balanced scorecard, alerts, working capital).
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetOverviewAsync(DssFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the profitability matrix by customer.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetProfitabilityAsync(DssFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the variance bridge.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetVarianceBridgeAsync(DssFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the actuals the server simulates on (a neutral scenario), so the offline simulator starts from the same figures.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetSimulationInputsAsync(DssFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the legal entities of the tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetLegalEntitiesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Generates the board pack on the server.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="format">pdf or xlsx.</param>
    /// <param name="locale">The language of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The file bytes.</returns>
    public Task<byte[]> ExportBoardPackAsync(DssFilter filter, string format, string locale, CancellationToken cancellationToken);
}

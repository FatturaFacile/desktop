// <copyright file="IFieldServiceApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Typed client of the Field Service Management endpoints of the web-api (`/api/v1/fsm`). The read methods return the raw `data`
/// of the answer and throw <see cref="HttpRequestException"/> when the call fails (without a status code when the server could
/// not be reached).
/// </summary>
public interface IFieldServiceApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads the dispatcher board of a time window.
    /// </summary>
    /// <param name="from">The start of the window.</param>
    /// <param name="to">The end of the window.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetBoardAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the list of work orders.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetWorkOrdersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads a work order with its parts and its service report.
    /// </summary>
    /// <param name="workOrderId">The work order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetWorkOrderAsync(string workOrderId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the maintenance contracts.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetContractsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the customer equipment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetEquipmentAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the service history of an asset.
    /// </summary>
    /// <param name="equipmentId">The equipment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetEquipmentHistoryAsync(string equipmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the offline bundle of a technician (the full bundle, not a delta).
    /// </summary>
    /// <param name="technicianId">The technician identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetTechnicianBundleAsync(string technicianId, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the PDF of a signed service report.
    /// </summary>
    /// <param name="reportId">The report identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The PDF.</returns>
    public Task<FieldServicePdf> DownloadReportPdfAsync(string reportId, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a write request below `fsm/` and returns the answer whatever its status. Network failures surface as
    /// <see cref="HttpRequestException"/>.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="route">The route below `fsm/` (for example "work-orders/{id}/dispatch").</param>
    /// <param name="jsonBody">The JSON body, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and body.</returns>
    public Task<FieldServiceApiReply> SendAsync(HttpMethod method, string route, string? jsonBody, CancellationToken cancellationToken);
}

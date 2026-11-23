// <copyright file="SupplierRebatesCall.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Runs a call to the web-api and turns every network or server failure into a <see cref="SupplierRebatesResult"/>, so no failure
/// escapes as an exception and an offline attempt never changes anything.
/// </summary>
internal static class SupplierRebatesCall
{
    private static readonly Dictionary<HttpStatusCode, SupplierRebatesStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = SupplierRebatesStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = SupplierRebatesStatus.Forbidden,
        [HttpStatusCode.BadRequest] = SupplierRebatesStatus.Rejected,
        [HttpStatusCode.Conflict] = SupplierRebatesStatus.Rejected,
        [HttpStatusCode.UnprocessableEntity] = SupplierRebatesStatus.Rejected,
    };

    /// <summary>
    /// Runs a call.
    /// </summary>
    /// <param name="call">The call.</param>
    /// <param name="cancellationToken">The cancellation token of the caller.</param>
    /// <returns>The result of the call, or the classified failure.</returns>
    internal static async Task<SupplierRebatesResult> RunAsync(Func<Task<SupplierRebatesResult>> call, CancellationToken cancellationToken)
    {
        try
        {
            return await call().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            // No status code means the server never answered.
            var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, SupplierRebatesStatus.Failed) : SupplierRebatesStatus.Offline;

            return new SupplierRebatesResult(status, Detail: ex.Message);
        }
        catch (JsonException ex)
        {
            return new SupplierRebatesResult(SupplierRebatesStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new SupplierRebatesResult(SupplierRebatesStatus.Offline, Detail: ex.Message);
        }
    }
}

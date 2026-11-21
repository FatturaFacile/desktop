// <copyright file="MesApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Mes;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Mes;

/// <summary>
/// HTTP client of the MES endpoints. It rides on the authenticated sync session (bearer token, tenant header and token refresh).
/// </summary>
/// <param name="api">The authenticated web-api client.</param>
public sealed class MesApiClient(ISyncApiClient api) : IMesApiClient
{
    private const string Base = "manufacturing/mes/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetBoardAsync(Guid workCenterId, CancellationToken cancellationToken) =>
        DataAsync(Base + "operator-board?work_center_id=" + workCenterId.ToString("D", CultureInfo.InvariantCulture), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetWorkCentersAsync(CancellationToken cancellationToken) =>
        DataAsync(Base + "work-centers", cancellationToken);

    /// <inheritdoc/>
    public async Task<MesApiReply> SendCommandAsync(string route, string jsonBody, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonBody);

        var reply = await api.SendAuthorizedAsync(HttpMethod.Post, route, jsonBody, cancellationToken).ConfigureAwait(false);

        return new MesApiReply(reply.Status, reply.Text);
    }

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("MES request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The MES answer has no data.");
    }
}

// <copyright file="ManufacturingApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// HTTP client of the manufacturing endpoints. It rides on the authenticated sync session (bearer token, tenant header and token refresh).
/// </summary>
/// <param name="api">The authenticated web-api client.</param>
public sealed class ManufacturingApiClient(ISyncApiClient api) : IManufacturingApiClient
{
    private const string Base = "manufacturing/";
    private const int PageSize = 100;
    private const int MaxPages = 200;
    private const string TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetListAsync(string resource, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        return ListAsync(resource, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> GetOeeSummaryAsync(Guid workCenterId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var url = Base + "oee/summary?work_center_id=" + workCenterId.ToString("D")
            + "&from_datetime=" + Uri.EscapeDataString(fromUtc.ToUniversalTime().ToString(TimestampFormat, CultureInfo.InvariantCulture))
            + "&to_datetime=" + Uri.EscapeDataString(toUtc.ToUniversalTime().ToString(TimestampFormat, CultureInfo.InvariantCulture));
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(reply);

        using var document = JsonDocument.Parse(reply.Text);
        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The manufacturing answer has no data.");
    }

    private static void EnsureSuccess(SyncApiReply reply)
    {
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Manufacturing request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }
    }

    private static bool IsLastPage(JsonElement root, int page) =>
        !root.TryGetProperty("meta", out var meta)
        || !meta.TryGetProperty("last_page", out var last)
        || !last.TryGetInt32(out var lastPage)
        || page >= lastPage;

    private async Task<string> ListAsync(string resource, CancellationToken cancellationToken)
    {
        List<string> items = [];
        for (var page = 1; page <= MaxPages; page++)
        {
            var url = Base + resource + "?per_page=" + PageSize.ToString(CultureInfo.InvariantCulture) + "&page=" + page.ToString(CultureInfo.InvariantCulture);
            var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
            EnsureSuccess(reply);

            using var document = JsonDocument.Parse(reply.Text);
            var root = document.RootElement;
            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The manufacturing answer has no data list.");
            }

            for (var i = 0; i < data.GetArrayLength(); i++)
            {
                items.Add(data[i].GetRawText());
            }

            if (data.GetArrayLength() == 0 || IsLastPage(root, page))
            {
                break;
            }
        }

        return "[" + string.Join(',', items) + "]";
    }
}

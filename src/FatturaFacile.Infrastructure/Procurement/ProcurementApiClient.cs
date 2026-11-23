// <copyright file="ProcurementApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Procurement;

/// <summary>
/// <see cref="IProcurementApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class ProcurementApiClient(ISyncApiClient api) : IProcurementApiClient
{
    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public async Task<string> GetAsync(string route, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, route, jsonBody: null, cancellationToken).ConfigureAwait(false);

        return reply.IsSuccess ? reply.Text : throw Failure(reply);
    }

    /// <inheritdoc/>
    public async Task<ProcurementApiReply> SendAsync(string method, string route, string? jsonBody, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        var verb = method switch
        {
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(method)),
        };

        var reply = await api.SendAuthorizedAsync(verb, route, jsonBody, cancellationToken).ConfigureAwait(false);

        return new ProcurementApiReply(reply.Status, reply.Text);
    }

    private static HttpRequestException Failure(SyncApiReply reply) =>
        new("Procurement request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
}

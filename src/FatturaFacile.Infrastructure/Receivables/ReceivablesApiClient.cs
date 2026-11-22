// <copyright file="ReceivablesApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Receivables;

/// <summary>
/// Calls the receivables endpoints of the web-api (invoicing module) with the authorized sync session.
/// </summary>
/// <param name="api">The authorized web-api client.</param>
public sealed class ReceivablesApiClient(ISyncApiClient api) : IReceivablesApiClient
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
    public async Task<ReceivablesFile> DownloadAsync(string route, string fileName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, route, jsonBody: null, cancellationToken).ConfigureAwait(false);

        return reply.IsSuccess && !reply.Content.IsEmpty ? new ReceivablesFile(fileName, reply.Content) : throw Failure(reply);
    }

    /// <inheritdoc/>
    public async Task<ReceivablesApiReply> SendAsync(string method, string route, string? jsonBody, CancellationToken cancellationToken)
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

        return new ReceivablesApiReply(reply.Status, reply.Text);
    }

    private static HttpRequestException Failure(SyncApiReply reply) =>
        new("Receivables request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
}

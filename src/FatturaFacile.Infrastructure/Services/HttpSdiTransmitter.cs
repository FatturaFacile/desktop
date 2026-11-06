// <copyright file="HttpSdiTransmitter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Invoicing.Sdi;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Generic adapter towards an accredited SDI intermediary exposing a simple REST interface (see docs/SDI-Intermediario.md):
/// PUT outbox/{file} sends an invoice, GET inbox lists the received files, DELETE inbox/{file} acknowledges one.
/// </summary>
public sealed class HttpSdiTransmitter : ISdiTransmitter
{
    private const char Slash = '/';
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(60) };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Uri _baseAddress;
    private readonly string? _apiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpSdiTransmitter"/> class.
    /// </summary>
    /// <param name="baseAddress">The address of the intermediary service.</param>
    /// <param name="apiKey">The access key (sent as bearer token).</param>
    public HttpSdiTransmitter(Uri baseAddress, string? apiKey)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);

        var text = baseAddress.AbsoluteUri;
        _baseAddress = text[^1] == Slash ? baseAddress : new Uri(text + Slash);
        _apiKey = apiKey;
    }

    /// <inheritdoc />
    public string Channel => "Intermediario " + _baseAddress.Host;

    /// <inheritdoc />
    public async Task<string?> SendAsync(string fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        using var request = Request(HttpMethod.Put, "outbox", fileName);
        request.Content = new ByteArrayContent(content.ToArray());
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SdiInboundFile>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        using var request = Request(HttpMethod.Get, "inbox", fileName: null);
        using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var items = JsonSerializer.Deserialize<List<HttpSdiInboxItem>>(json, JsonOptions) ?? [];
        return [.. items.Select(static i => new SdiInboundFile(i.FileName, Convert.FromBase64String(i.Content)))];
    }

    /// <inheritdoc />
    public Task AcknowledgeAsync(SdiInboundFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return AcknowledgeInternalAsync(file.FileName, cancellationToken);
    }

    private async Task AcknowledgeInternalAsync(string fileName, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Delete, "inbox", fileName);
        using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
    }

    private HttpRequestMessage Request(HttpMethod method, string folder, string? fileName)
    {
        var path = fileName is null ? folder : folder + Slash + Uri.EscapeDataString(Path.GetFileName(fileName));
        HttpRequestMessage request = new(method, new Uri(_baseAddress, path));

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        return request;
    }
}

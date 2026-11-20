// <copyright file="CopilotApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Infrastructure.Copilot;

/// <summary>
/// Typed <see cref="HttpClient"/> of the ERP Copilot REST/SSE API. Sends the Bearer token and the X-Tenant-Id header
/// with every call and reads the {success, message, data, meta} envelope.
/// </summary>
public sealed class CopilotApiClient : ICopilotApiClient
{
    private const string Prefix = "v1/copilot/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;
    private readonly CopilotOptions _options;
    private readonly ICurrentTenantService _tenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotApiClient"/> class.
    /// </summary>
    /// <param name="client">The HTTP client (its base address is set from the options when empty).</param>
    /// <param name="options">The connection settings.</param>
    /// <param name="tenant">The active tenant.</param>
    public CopilotApiClient(HttpClient client, CopilotOptions options, ICurrentTenantService tenant)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        _client = client;
        _options = options;
        _tenant = tenant;

        client.BaseAddress ??= options.GetBaseAddress();
    }

    /// <inheritdoc />
    public bool IsConfigured => _client.BaseAddress is not null;

    /// <summary>
    /// Builds the JSON body of a prompt, injecting the active context (route, entity_type, entity_id).
    /// </summary>
    /// <param name="prompt">The prompt.</param>
    /// <param name="locale">The locale.</param>
    /// <param name="context">The active context.</param>
    /// <returns>The body object.</returns>
    public static object BuildMessageBody(string prompt, string locale, CopilotContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new
        {
            prompt,
            locale,
            context = new { route = context.Route, entity_type = context.EntityType, entity_id = context.EntityId },
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CopilotConversation>> GetConversationsAsync(CancellationToken cancellationToken = default)
    {
        var data = await SendAsync(HttpMethod.Get, "conversations", body: null, cancellationToken).ConfigureAwait(false);
        return CopilotApiMapper.ToConversations(data);
    }

    /// <inheritdoc />
    public Task<CopilotConversation> CreateConversationAsync(string? title, string locale, CopilotContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var body = new
        {
            title,
            locale,
            context_route = context.Route,
            context_entity_type = context.EntityType,
            context_entity_id = context.EntityId,
        };

        return PostConversationAsync(body, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CopilotConversation> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default) =>
        CopilotApiMapper.ToConversation(await SendAsync(HttpMethod.Get, "conversations/" + Uri.EscapeDataString(conversationId), body: null, cancellationToken).ConfigureAwait(false));

    /// <inheritdoc />
    public async Task DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default) =>
        _ = await SendAsync(HttpMethod.Delete, "conversations/" + Uri.EscapeDataString(conversationId), body: null, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public IAsyncEnumerable<CopilotStreamEvent> StreamMessageAsync(string conversationId, string prompt, string locale, CopilotContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(conversationId);

        return ReadStreamAsync(conversationId, BuildMessageBody(prompt, locale, context), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CopilotMessage> ConfirmActionAsync(string messageId, IReadOnlyDictionary<string, object?>? overrides, CancellationToken cancellationToken = default)
    {
        var body = overrides is { Count: > 0 } ? new { overrides } : (object)new { };
        return CopilotApiMapper.ToMessage(await SendAsync(HttpMethod.Post, "actions/" + Uri.EscapeDataString(messageId) + "/confirm", body, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<CopilotMessage> RejectActionAsync(string messageId, string? reason, CancellationToken cancellationToken = default)
    {
        var body = string.IsNullOrWhiteSpace(reason) ? (object)new { } : new { reason };
        return CopilotApiMapper.ToMessage(await SendAsync(HttpMethod.Post, "actions/" + Uri.EscapeDataString(messageId) + "/reject", body, cancellationToken).ConfigureAwait(false));
    }

    private static async Task<CopilotApiException> ToExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var status = (int)response.StatusCode;
        var message = string.Empty;

        try
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(text))
            {
                using var document = JsonDocument.Parse(text);
                message = ReadMessage(document.RootElement);
            }
        }
        catch (JsonException)
        {
            // The body is not JSON: the status code is enough.
        }

        return new CopilotApiException(CopilotApiException.KindFromStatus(status), status, message);
    }

    private static string ReadMessage(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        foreach (var name in new[] { "message", "detail" })
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private async IAsyncEnumerable<CopilotStreamEvent> ReadStreamAsync(string conversationId, object body, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, "conversations/" + Uri.EscapeDataString(conversationId) + "/messages", body, "text/event-stream");
        using var response = await SendRawAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var raw in CopilotSseParser.ParseAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            if (CopilotApiMapper.ToStreamEvent(raw) is { } streamEvent)
            {
                yield return streamEvent;
            }
        }
    }

    private async Task<CopilotConversation> PostConversationAsync(object body, CancellationToken cancellationToken) =>
        CopilotApiMapper.ToConversation(await SendAsync(HttpMethod.Post, "conversations", body, cancellationToken).ConfigureAwait(false));

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body, string accept)
    {
        if (_client.BaseAddress is null)
        {
            throw new CopilotApiException(CopilotApiErrorKind.Unauthorized, statusCode: null, message: "Copilot is not configured.");
        }

        HttpRequestMessage request = new(method, Prefix + path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        request.Headers.Add("X-Tenant-Id", _tenant.TenantId.ToString());

        if (!string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return request;
    }

    private async Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request, HttpCompletionOption completion, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await _client.SendAsync(request, completion, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new CopilotApiException(CopilotApiErrorKind.Offline, statusCode: null, message: ex.Message, innerException: ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CopilotApiException(CopilotApiErrorKind.Offline, statusCode: null, message: ex.Message, innerException: ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            using (response)
            {
                throw await ToExceptionAsync(response, cancellationToken).ConfigureAwait(false);
            }
        }

        return response;
    }

    private async Task<JsonElement> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, path, body, "application/json");
        using var response = await SendRawAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;

        return root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) ? data.Clone() : root.Clone();
    }
}

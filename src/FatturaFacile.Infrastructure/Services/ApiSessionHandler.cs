// <copyright file="ApiSessionHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// HTTP delegating handler of the API client: sends the Sanctum bearer token and reacts to what the server withdraws. A 401
/// (token or session revoked) purges the token and the cached permissions at once; a 403 with the code permission_denied,
/// mfa_required or company_access_revoked is reported so the shell can reload the permissions, ask for the second factor
/// or leave the company.
/// </summary>
/// <param name="session">The API session.</param>
public sealed class ApiSessionHandler(IApiSession session) : DelegatingHandler
{
    private static readonly string[] AnonymousPaths = ["/auth/login", "/auth/two-factor-challenge", "/auth/passkeys/login", "/auth/sso"];

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendCoreAsync(request, cancellationToken);
    }

    private static bool IsAnonymous(HttpRequestMessage request) =>
        request.RequestUri is { } uri && AnonymousPaths.Any(path => uri.AbsolutePath.Contains(path, StringComparison.OrdinalIgnoreCase));

    private static async Task<string?> ReadCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            // Buffer the body so the caller can still read it after the handler looked at the code.
            await response.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(body);

            return document.RootElement.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String ? code.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (session.BearerToken is { } token && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsAnonymous(request))
        {
            await session.EndAsync(ApiProblemKind.SessionRevoked, cancellationToken).ConfigureAwait(false);
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            await HandleForbiddenAsync(response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    private async Task HandleForbiddenAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var code = await ReadCodeAsync(response, cancellationToken).ConfigureAwait(false);

        if (string.Equals(code, "permission_denied", StringComparison.Ordinal))
        {
            session.Report(ApiProblemKind.PermissionDenied);
        }
        else if (string.Equals(code, "mfa_required", StringComparison.Ordinal))
        {
            session.Report(ApiProblemKind.MfaRequired);
        }
        else if (string.Equals(code, "company_access_revoked", StringComparison.Ordinal))
        {
            await session.EndAsync(ApiProblemKind.CompanyAccessRevoked, cancellationToken).ConfigureAwait(false);
        }
    }
}

// <copyright file="SupplierRebatesApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.SupplierRebates;

/// <summary>
/// <see cref="ISupplierRebatesApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class SupplierRebatesApiClient(ISyncApiClient api) : ISupplierRebatesApiClient
{
    private const string Rebates = "rebates/";
    private const string EmptyObject = "{}";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetAgreementsAsync(CancellationToken cancellationToken) =>
        ReadAsync(Rebates + "agreements?direction=" + RebateCodes.DirectionPurchase + "&per_page=100", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetAgreementAsync(string agreementId, CancellationToken cancellationToken) =>
        ReadAsync(Rebates + "agreements/" + Uri.EscapeDataString(agreementId), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetProgressAsync(string agreementId, CancellationToken cancellationToken) =>
        ReadAsync(Rebates + "agreements/" + Uri.EscapeDataString(agreementId) + "/progress", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetRunsAsync(string agreementId, CancellationToken cancellationToken) =>
        ReadAsync(Rebates + "agreements/" + Uri.EscapeDataString(agreementId) + "/accrual-runs", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetProposalsAsync(string agreementId, CancellationToken cancellationToken) =>
        ReadAsync(Rebates + "settlement-proposals?per_page=100&agreement_id=" + Uri.EscapeDataString(agreementId), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetBlanketContractsAsync(CancellationToken cancellationToken) =>
        ReadAsync("procurement/contracts?per_page=100", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetBlanketBalanceAsync(string contractId, CancellationToken cancellationToken) =>
        ReadAsync("procurement/contracts/" + Uri.EscapeDataString(contractId) + "/balance", cancellationToken);

    /// <inheritdoc/>
    public async Task<string> GetPassiveInvoicesAsync(CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, "purchases/passive-invoices?per_page=100", jsonBody: null, cancellationToken).ConfigureAwait(false);

        return reply.IsSuccess ? reply.Text : throw Failure(reply);
    }

    /// <inheritdoc/>
    public Task<string> CreateAgreementAsync(string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "agreements", jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> UpdateAgreementAsync(string agreementId, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Put, Rebates + "agreements/" + Uri.EscapeDataString(agreementId), jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> DeleteAgreementAsync(string agreementId, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Delete, Rebates + "agreements/" + Uri.EscapeDataString(agreementId), jsonBody: null, cancellationToken);

    /// <inheritdoc/>
    public Task<string> ChangeAgreementStatusAsync(string agreementId, bool activate, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "agreements/" + Uri.EscapeDataString(agreementId) + (activate ? "/activate" : "/close"), EmptyObject, cancellationToken);

    /// <inheritdoc/>
    public Task<string> AddContributionAsync(string agreementId, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "agreements/" + Uri.EscapeDataString(agreementId) + "/contributions", jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> UpdateContributionAsync(string contributionId, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Put, Rebates + "contributions/" + Uri.EscapeDataString(contributionId), jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> DeleteContributionAsync(string contributionId, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Delete, Rebates + "contributions/" + Uri.EscapeDataString(contributionId), jsonBody: null, cancellationToken);

    /// <inheritdoc/>
    public Task<string> RunAccrualAsync(string agreementId, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "agreements/" + Uri.EscapeDataString(agreementId) + "/accrual-runs", jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> ReverseRunAsync(string runId, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "accrual-runs/" + Uri.EscapeDataString(runId) + "/reverse", jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> ProposeSettlementAsync(string agreementId, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "agreements/" + Uri.EscapeDataString(agreementId) + "/settlement-proposals", jsonBody, cancellationToken);

    /// <inheritdoc/>
    public Task<string> AdvanceSettlementAsync(string proposalId, SettlementAction action, string jsonBody, CancellationToken cancellationToken) =>
        WriteAsync(HttpMethod.Post, Rebates + "settlement-proposals/" + Uri.EscapeDataString(proposalId) + "/" + RebateWire.ToWire(action), jsonBody, cancellationToken);

    private static HttpRequestException Failure(SyncApiReply reply) =>
        new(ServerMessage(reply) ?? "Supplier rebates request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);

    // Reads the message of an error answer: the Laravel validation message of the first field, the `message`, or the RFC 7807 `detail`.
    private static string? ServerMessage(SyncApiReply reply)
    {
        try
        {
            using var document = JsonDocument.Parse(reply.Text);
            var root = document.RootElement;

            return FirstError(root) ?? Text(root, "message") ?? Text(root, "detail") ?? Text(root, "title");
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? FirstError(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var field in errors.EnumerateObject())
        {
            if (field.Value.ValueKind == JsonValueKind.Array && field.Value.GetArrayLength() > 0 && field.Value[0].ValueKind == JsonValueKind.String)
            {
                return field.Value[0].GetString();
            }
        }

        return null;
    }

    private static string? Text(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private async Task<string> ReadAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw Failure(reply);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The supplier rebates answer has no data.");
    }

    private async Task<string> WriteAsync(HttpMethod method, string url, string? jsonBody, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(method, url, jsonBody, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw Failure(reply);
        }

        // A 204 (delete) has no body: there is nothing to read.
        if (reply.Status == HttpStatusCode.NoContent || reply.Content.IsEmpty)
        {
            return EmptyObject;
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data) ? data.GetRawText() : EmptyObject;
    }
}

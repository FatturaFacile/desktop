// <copyright file="CustomerLifecycleApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.CustomerLifecycle;

/// <summary>
/// HTTP client of the customer 360 endpoints. It rides on the authenticated sync session (bearer token, tenant header and token refresh).
/// </summary>
/// <param name="api">The authenticated web-api client.</param>
public sealed class CustomerLifecycleApiClient(ISyncApiClient api) : ICustomerLifecycleApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    public async Task<Guid?> FindAccountIdAsync(Guid contactId, CancellationToken cancellationToken)
    {
        var data = await DataAsync("crm/accounts?contact_id=" + contactId.ToString("D"), cancellationToken).ConfigureAwait(false);

        // The list is either a plain array or a paginator whose own "data" holds the rows.
        var rows = data.ValueKind == JsonValueKind.Object && data.TryGetProperty("data", out var inner) ? inner : data;
        if (rows.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var row in rows.EnumerateArray())
        {
            if (row.TryGetProperty("id", out var id) && Guid.TryParse(id.GetString(), out var accountId))
            {
                return accountId;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<CustomerLifecycleSummary> GetSummaryAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var data = await DataAsync("crm/customers/" + accountId.ToString("D") + "/360", cancellationToken).ConfigureAwait(false);

        return data.Deserialize<CustomerLifecycleSummary>(Json) ?? throw new JsonException("The customer 360 answer is empty.");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerTimelineEvent>> GetTimelineAsync(Guid accountId, IReadOnlyCollection<string> types, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(types);

        StringBuilder url = new();
        _ = url.Append("crm/customers/").Append(accountId.ToString("D")).Append("/timeline?limit=50");
        foreach (var type in types)
        {
            _ = url.Append("&types%5B%5D=").Append(Uri.EscapeDataString(type));
        }

        var data = await DataAsync(url.ToString(), cancellationToken).ConfigureAwait(false);

        return data.Deserialize<List<CustomerTimelineEvent>>(Json) ?? [];
    }

    private async Task<JsonElement> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, body: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Customer 360 request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.Clone()
            : throw new JsonException("The customer 360 answer has no data.");
    }
}

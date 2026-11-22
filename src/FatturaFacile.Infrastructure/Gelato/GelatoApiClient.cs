// <copyright file="GelatoApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Gelato;
using FatturaFacile.Domain.Common;
using FatturaFacile.Infrastructure.Copilot;

namespace FatturaFacile.Infrastructure.Gelato;

/// <summary>
/// Typed <see cref="HttpClient"/> of the gelato API. Sends the Bearer token and the X-Tenant-Id header with every call and
/// reads the {success, message, data, meta} envelope.
/// </summary>
public sealed class GelatoApiClient : IGelatoApiClient
{
    private const string Prefix = "v1/gelato/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;
    private readonly CopilotOptions _options;
    private readonly ICurrentTenantService _tenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="GelatoApiClient"/> class.
    /// </summary>
    /// <param name="client">The HTTP client (its base address is set from the options when empty).</param>
    /// <param name="options">The connection settings of the ERP API.</param>
    /// <param name="tenant">The active tenant.</param>
    public GelatoApiClient(HttpClient client, CopilotOptions options, ICurrentTenantService tenant)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        _client = client;
        _options = options;
        _tenant = tenant;

        client.BaseAddress ??= options.GetBaseAddress();
    }

    /// <summary>
    /// Reads a localized name object ({it, de, fr, en}) in the language, falling back to English and then to any value.
    /// </summary>
    /// <param name="name">The JSON name.</param>
    /// <param name="locale">The language.</param>
    /// <returns>The name.</returns>
    public static string LocalizedName(JsonElement name, string locale)
    {
        if (name.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        foreach (var key in new[] { locale, "en" })
        {
            if (name.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(value.GetString()))
            {
                return value.GetString()!;
            }
        }

        foreach (var property in name.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GelatoRecipeSummary>> GetRecipesAsync(string locale, CancellationToken cancellationToken = default)
    {
        var data = await SendAsync(HttpMethod.Get, "recipes", body: null, cancellationToken).ConfigureAwait(false);

        return data.ValueKind != JsonValueKind.Array
            ? []
            : data.EnumerateArray().Select(r => new GelatoRecipeSummary(
                r.GetProperty("id").GetString() ?? string.Empty,
                r.GetProperty("code").GetString() ?? string.Empty,
                LocalizedName(r.GetProperty("name"), locale),
                Number(r, "batch_weight_grams"),
                Number(r, "calculated_pod_percent"),
                Number(r, "calculated_pac_index"),
                Number(r, "food_cost_per_kg"))).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GelatoRecipeLineInfo>> GetRecipeLinesAsync(string recipeId, string locale, CancellationToken cancellationToken = default)
    {
        var recipe = await SendAsync(HttpMethod.Get, "recipes/" + Uri.EscapeDataString(recipeId), body: null, cancellationToken).ConfigureAwait(false);

        if (!recipe.TryGetProperty("lines", out var lines) || lines.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var ingredients = await SendAsync(HttpMethod.Get, "ingredients", body: null, cancellationToken).ConfigureAwait(false);
        var names = ingredients.ValueKind == JsonValueKind.Array
            ? ingredients.EnumerateArray().ToDictionary(i => i.GetProperty("id").GetString() ?? string.Empty, i => LocalizedName(i.GetProperty("name"), locale), StringComparer.Ordinal)
            : [];

        return lines.EnumerateArray()
            .Where(l => l.TryGetProperty("ingredient_id", out var id) && id.ValueKind == JsonValueKind.String)
            .Select(l =>
            {
                var id = l.GetProperty("ingredient_id").GetString() ?? string.Empty;

                return new GelatoRecipeLineInfo(id, names.GetValueOrDefault(id, id), Number(l, "grams"));
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task RecordScaleReadingAsync(string ingredientId, decimal grams, CancellationToken cancellationToken = default)
    {
        _ = await SendAsync(HttpMethod.Post, "scale-readings", new { ingredient_id = ingredientId, grams }, cancellationToken).ConfigureAwait(false);
    }

    private static decimal Number(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return 0m;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0m,
        };
    }

    private async Task<JsonElement> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        if (_client.BaseAddress is null)
        {
            throw new InvalidOperationException("The ERP API address is not configured.");
        }

        using HttpRequestMessage request = new(method, Prefix + path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-Tenant-Id", _tenant.TenantId.ToString());

        if (!string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
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

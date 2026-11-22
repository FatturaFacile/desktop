// <copyright file="TemplateParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Reads the JSON parts of a template, as the web-api sends them and as they are cached locally.
/// </summary>
public static class TemplateParser
{
    /// <summary>
    /// Builds a template definition from a cached entity.
    /// </summary>
    /// <param name="template">The cached template.</param>
    /// <param name="language">The language of the name: it, de, fr or en.</param>
    /// <returns>The definition.</returns>
    public static TemplateDefinition ToDefinition(AccountingTemplate template, string language)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new TemplateDefinition(
            template.Id,
            template.Code,
            PickName(template.NamesJson, language, template.Code),
            template.LedgerType,
            ParseParameters(template.ParametersJson),
            ParseLines(template.LinesJson));
    }

    /// <summary>
    /// Reads the parameter array of a template.
    /// </summary>
    /// <param name="json">The JSON array of key/type objects.</param>
    /// <returns>The parameters.</returns>
    public static IReadOnlyList<TemplateParameter> ParseParameters(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var parameters = LedgerJson.Items(document.RootElement).Select(static p => new TemplateParameter(LedgerJson.Text(p, "key"), LedgerJson.Text(p, "type")));

        return [.. parameters];
    }

    /// <summary>
    /// Reads the line array of a template.
    /// </summary>
    /// <param name="json">The JSON array of account/side/amount/description objects.</param>
    /// <returns>The lines.</returns>
    public static IReadOnlyList<TemplateLineDefinition> ParseLines(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var lines = LedgerJson.Items(document.RootElement).Select(static l => new TemplateLineDefinition(
            LedgerJson.Text(l, "account"),
            LedgerJson.Text(l, "side"),
            LedgerJson.Text(l, "amount"),
            LedgerJson.OptionalText(l, "description")));

        return [.. lines];
    }

    /// <summary>
    /// Picks the name of a template in a language, falling back to English then the code.
    /// </summary>
    /// <param name="namesJson">The JSON object of names keyed by language.</param>
    /// <param name="language">The wanted language.</param>
    /// <param name="fallback">The text used when no name exists.</param>
    /// <returns>The name.</returns>
    public static string PickName(string namesJson, string language, string fallback)
    {
        ArgumentNullException.ThrowIfNull(namesJson);

        using var document = JsonDocument.Parse(namesJson);
        var root = document.RootElement;

        foreach (var key in new[] { language, "en" })
        {
            if (root.ValueKind == JsonValueKind.Object && LedgerJson.OptionalText(root, key) is { Length: > 0 } text)
            {
                return text;
            }
        }

        return fallback;
    }
}

// <copyright file="AnalyticalEntryDraft.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// A manual analytical adjustment captured on the desktop, to be queued offline and sent to `POST /analytical/entries`. It writes the
/// management ledger only, never the statutory ledger.
/// </summary>
/// <param name="PostingDate">The posting date.</param>
/// <param name="Description">The description.</param>
/// <param name="Lines">The lines.</param>
public sealed record AnalyticalEntryDraft(DateOnly PostingDate, string? Description, IReadOnlyList<AnalyticalEntryLineDraft> Lines)
{
    /// <summary>
    /// Checks the draft: at least one line, a valid cost element on each, a target on each plain line and valid vectors on each
    /// split line.
    /// </summary>
    /// <returns>The error codes (resource keys); empty when the draft can be queued.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (Lines.Count == 0)
        {
            errors.Add("Analytical_EntryNeedsLines");

            return errors;
        }

        foreach (var line in Lines)
        {
            if (!AnalyticalContract.CostElementTypes.Contains(line.CostElementType, StringComparer.Ordinal))
            {
                errors.Add("Analytical_InvalidCostElement");
            }

            if (line.Vectors is { Count: > 0 })
            {
                errors.AddRange(AllocationVectorSplitter.Split(line.Amount, line.Vectors).Errors);
            }
            else if (line.CostCenterId is null && line.ProfitCenterId is null)
            {
                errors.Add("Analytical_VectorTargetRequired");
            }
        }

        return [.. errors.Distinct(StringComparer.Ordinal)];
    }

    /// <summary>
    /// Builds the JSON body of the API request.
    /// </summary>
    /// <returns>The JSON text.</returns>
    public string ToApiJson()
    {
        var lines = new JsonArray();
        foreach (var line in Lines)
        {
            var node = new JsonObject
            {
                ["amount"] = line.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                ["cost_element_type"] = line.CostElementType,
            };
            if (line.CostCenterId is not null)
            {
                node["cost_center_id"] = line.CostCenterId;
            }

            if (line.ProfitCenterId is not null)
            {
                node["profit_center_id"] = line.ProfitCenterId;
            }

            if (line.Vectors is { Count: > 0 })
            {
                node["vectors"] = new JsonArray([.. line.Vectors.Select(ToVectorNode)]);
            }

            lines.Add(node);
        }

        var root = new JsonObject
        {
            ["posting_date"] = PostingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["lines"] = lines,
        };
        if (!string.IsNullOrWhiteSpace(Description))
        {
            root["description"] = Description;
        }

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static JsonObject ToVectorNode(AllocationVector vector)
    {
        var node = new JsonObject();
        AddIfPresent(node, "cost_center_id", vector.CostCenterId);
        AddIfPresent(node, "profit_center_id", vector.ProfitCenterId);
        AddIfPresent(node, "project_id", vector.ProjectId);
        AddIfPresent(node, "product_line_id", vector.ProductLineId);
        AddIfPresent(node, "cost_element_type", vector.CostElementType);
        if (vector.Percentage is { } percentage)
        {
            node["percentage"] = percentage;
        }

        if (vector.Amount is { } amount)
        {
            node["amount"] = amount.ToString("0.00", CultureInfo.InvariantCulture);
        }

        return node;
    }

    private static void AddIfPresent(JsonObject node, string name, string? value)
    {
        if (value is not null)
        {
            node[name] = value;
        }
    }
}

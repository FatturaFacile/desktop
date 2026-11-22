// <copyright file="HelpdeskParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// Parses the helpdesk answers of the web-api (the data member) into the models of the workbench. Numbers may arrive as strings.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "The DTOs are created by the JSON deserializer.")]
public static class HelpdeskParser
{
    private const int SummaryLength = 240;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>
    /// Parses the breach-risk list.
    /// </summary>
    /// <param name="json">The JSON array.</param>
    /// <returns>The tickets, in the order of the server.</returns>
    public static IReadOnlyList<HelpdeskTicketRisk> ParseBreachRisk(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return [.. ReadList<TicketDto>(json).Where(ticket => ticket.TicketId is not null).Select(ToTicket)];
    }

    /// <summary>
    /// Parses the SLA metrics.
    /// </summary>
    /// <param name="json">The JSON object.</param>
    /// <returns>The metrics.</returns>
    public static HelpdeskMetrics ParseMetrics(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var metrics = Read<MetricsDto>(json);

        return new HelpdeskMetrics(metrics.GroupBy ?? string.Empty, [.. (metrics.Groups ?? []).Select(ToGroup)]);
    }

    /// <summary>
    /// Parses the customer 360 context of a ticket.
    /// </summary>
    /// <param name="json">The JSON object.</param>
    /// <returns>The context.</returns>
    public static HelpdeskCustomerContext ParseCustomerContext(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var context = Read<ContextDto>(json);
        var invoices = context.OpenInvoices;

        return new HelpdeskCustomerContext(
            context.CustomerId,
            [.. (context.Contracts ?? []).Select(ToContract)],
            [.. (context.Assets ?? []).Select(asset => new HelpdeskAsset(asset.Id, asset.SerialNumber ?? string.Empty, Day(asset.WarrantyExpiryDate)))],
            new HelpdeskOpenInvoices(invoices?.Count ?? 0, invoices?.OverdueCount ?? 0, invoices?.TotalGross ?? 0m),
            [.. (context.RecentTickets ?? []).Select(ticket => new HelpdeskRecentTicket(ticket.Id, ticket.TicketNumber ?? ticket.Number ?? string.Empty, ticket.Subject ?? string.Empty, ticket.Status ?? string.Empty))]);
    }

    /// <summary>
    /// Parses the escalation history of a ticket.
    /// </summary>
    /// <param name="json">The JSON array.</param>
    /// <returns>The steps, most recent first.</returns>
    public static IReadOnlyList<HelpdeskEscalation> ParseEscalations(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return
        [
            .. ReadList<EscalationDto>(json)
                .Select(step => new HelpdeskEscalation(step.Id, step.Target ?? string.Empty, step.ThresholdPercent ?? 0d, step.ElapsedPercent ?? 0d, step.Action ?? string.Empty, step.Outcome ?? string.Empty, Utc(step.FiredAt)))
                .OrderByDescending(step => step.FiredAt),
        ];
    }

    /// <summary>
    /// Parses a knowledge base search; the list may be the data itself or sit in a data member of a page.
    /// </summary>
    /// <param name="json">The JSON.</param>
    /// <returns>The articles.</returns>
    public static IReadOnlyList<HelpdeskKbArticle> ParseKnowledge(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return [.. ReadList<ArticleDto>(json).Select(ToArticle).Where(article => article.Title.Length > 0)];
    }

    /// <summary>
    /// Parses the time entry that the server created.
    /// </summary>
    /// <param name="json">The body of the answer (with the data member).</param>
    /// <returns>How the entry was settled, or null when the answer has no billing status.</returns>
    public static HelpdeskTimeOutcome? ParseTimeEntry(string json)
    {
        var data = Try<EnvelopeDto<TimeEntryDto>>(json)?.Data;

        return string.IsNullOrEmpty(data?.BillingStatus)
            ? null
            : new HelpdeskTimeOutcome(data.BillingStatus, data.ContractHours ?? 0m, data.BillableHours ?? 0m, data.TotalCharge ?? 0m);
    }

    /// <summary>
    /// Reads the tracking token of the customer from the answer of an RMA escalation.
    /// </summary>
    /// <param name="json">The body of the answer (with the meta member).</param>
    /// <returns>The token, or null.</returns>
    public static string? ParseTrackingToken(string json)
    {
        var token = Try<EnvelopeDto<TimeEntryDto>>(json)?.Meta?.TrackingToken;

        return string.IsNullOrEmpty(token) ? null : token;
    }

    private static T? Try<T>(string json)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static T Read<T>(string json)
        where T : class =>
        JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException("The helpdesk answer is empty.");

    private static List<T> ReadList<T>(string json) =>
        json.AsSpan().TrimStart().StartsWith("[", StringComparison.Ordinal)
            ? JsonSerializer.Deserialize<List<T>>(json, Options) ?? []
            : JsonSerializer.Deserialize<PageDto<T>>(json, Options)?.Data ?? [];

    private static DateTime? Utc(DateTime? value) => value switch
    {
        null => null,
        { Kind: DateTimeKind.Unspecified } instant => DateTime.SpecifyKind(instant, DateTimeKind.Utc),
        { } instant => instant.ToUniversalTime(),
    };

    private static DateOnly? Day(DateTime? value) => value is { } instant ? DateOnly.FromDateTime(instant) : null;

    private static HelpdeskTicketRisk ToTicket(TicketDto ticket) => new(
        ticket.TicketId ?? Guid.Empty,
        ticket.TicketNumber ?? string.Empty,
        ticket.Subject ?? string.Empty,
        ticket.Priority ?? string.Empty,
        ticket.Status ?? string.Empty,
        ticket.CustomerId,
        ticket.AssignedTeamId,
        ticket.AssignedTechnicianId,
        ticket.RiskPercent ?? 0d,
        [.. (ticket.Targets ?? []).Select(target => new HelpdeskBreachTarget(target.Target ?? string.Empty, Utc(target.DueAt), target.ElapsedPercent ?? 0d, target.State ?? HelpdeskBreachTarget.AtRisk))]);

    private static HelpdeskMetricGroup ToGroup(GroupDto group) => new(
        group.Key?.ToString() ?? string.Empty,
        group.Tickets ?? 0,
        group.MttfrMinutes,
        group.MttrMinutes,
        group.FirstResponseCompliancePercent,
        group.ResolutionCompliancePercent,
        new HelpdeskCsat(group.Csat?.Responses ?? 0, group.Csat?.Average));

    private static HelpdeskContract ToContract(ContractDto contract) => new(
        contract.Id,
        contract.Name ?? string.Empty,
        contract.Kind ?? string.Empty,
        contract.IncludedHours,
        contract.ConsumedHours,
        contract.RemainingHours,
        Day(contract.EndDate));

    private static HelpdeskKbArticle ToArticle(ArticleDto article)
    {
        var summary = article.Summary ?? article.Excerpt ?? article.Content ?? string.Empty;

        return new HelpdeskKbArticle(
            article.Id?.ToString() ?? string.Empty,
            article.Title ?? string.Empty,
            summary.Length > SummaryLength ? $"{summary[..SummaryLength]}..." : summary);
    }

    private sealed record PageDto<T>(List<T>? Data);

    private sealed record EnvelopeDto<T>(T? Data, MetaDto? Meta);

    private sealed record MetaDto(string? TrackingToken);

    private sealed record TimeEntryDto(string? BillingStatus, decimal? ContractHours, decimal? BillableHours, decimal? TotalCharge);

    private sealed record TargetDto(string? Target, DateTime? DueAt, double? ElapsedPercent, string? State);

    private sealed record TicketDto(
        Guid? TicketId,
        string? TicketNumber,
        string? Subject,
        string? Priority,
        string? Status,
        Guid? CustomerId,
        Guid? AssignedTeamId,
        Guid? AssignedTechnicianId,
        double? RiskPercent,
        List<TargetDto>? Targets);

    private sealed record CsatDto(int? Responses, double? Average);

    private sealed record GroupDto(
        JsonElement? Key,
        int? Tickets,
        double? MttfrMinutes,
        double? MttrMinutes,
        double? FirstResponseCompliancePercent,
        double? ResolutionCompliancePercent,
        CsatDto? Csat);

    private sealed record MetricsDto(string? GroupBy, List<GroupDto>? Groups);

    private sealed record ContractDto(Guid? Id, string? Name, string? Kind, decimal? IncludedHours, decimal? ConsumedHours, decimal? RemainingHours, DateTime? EndDate);

    private sealed record AssetDto(Guid? Id, string? SerialNumber, DateTime? WarrantyExpiryDate);

    private sealed record InvoicesDto(int? Count, int? OverdueCount, decimal? TotalGross);

    private sealed record RecentTicketDto(Guid? Id, string? TicketNumber, string? Number, string? Subject, string? Status);

    private sealed record ContextDto(Guid? CustomerId, List<ContractDto>? Contracts, List<AssetDto>? Assets, InvoicesDto? OpenInvoices, List<RecentTicketDto>? RecentTickets);

    private sealed record EscalationDto(Guid? Id, string? Target, double? ThresholdPercent, double? ElapsedPercent, string? Action, string? Outcome, DateTime? FiredAt);

    private sealed record ArticleDto(JsonElement? Id, string? Title, string? Summary, string? Excerpt, string? Content);
}

// <copyright file="CopilotApiMapperTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.Text.Json;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Infrastructure.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the mapping of the Copilot API resources to application models.
/// </summary>
public sealed class CopilotApiMapperTests
{
    private const string MessageJson = """{"id":"m1","conversation_id":"c1","role":"assistant","content_markdown":"Fattura #Invoice-2026-0042 vedi [LIVA Art. 25]","citations":[{"type":"regulatory","label":"[LIVA Art. 25]","title":"LIVA","section_ref":"Art. 25","jurisdiction":"CH"},{"type":"entity","label":"#Invoice-2026-0042","entity_type":"invoice","entity_id":"42","route":"/invoices/42"}],"visualization":{"type":"bar","title":"Fatturato","columns":["month_date","total"],"rows":[{"month_date":"2026-01","total":10.5},{"month_date":"2026-02","total":20}],"x":"month_date","labels":["2026-01","2026-02"],"datasets":[{"name":"total","data":[10.5,20]}]},"proposed_action":{"tool":"create_invoice","module":"invoicing","status":"pending","summary":"Crea fattura","arguments":{"customer":"ACME","items":[{"description":"Consulenza","quantity":2,"billable":true}]},"details":{"Cliente":"ACME","Totale":200},"editable_fields":["items.0.quantity"]},"action_status":"pending","token_usage":null,"created_at":"2026-11-20T10:00:00Z"}""";

    /// <summary>
    /// Citations, visualization and proposed action of a message are mapped, arguments are flattened to dotted paths.
    /// </summary>
    [Fact]
    public void ToMessage_MapsCitationsVisualizationAndAction()
    {
        using var document = JsonDocument.Parse(MessageJson);
        var message = CopilotApiMapper.ToMessage(document.RootElement);

        Assert.Equal("m1", message.Id);
        Assert.Equal(2, message.Citations.Count);
        Assert.True(message.Citations[0].IsRegulatory);
        Assert.Equal("CH", message.Citations[0].Jurisdiction);
        Assert.True(message.Citations[1].IsEntity);
        Assert.Equal("/invoices/42", message.Citations[1].Route);

        var visualization = Assert.IsType<CopilotVisualization>(message.Visualization);
        Assert.True(visualization.IsChart);
        Assert.Equal("10.5", visualization.Rows[0]["total"]);
        var series = Assert.Single(visualization.GetSeries());
        Assert.Equal(new CopilotDataPoint("2026-02", 20d), series.Points[1]);

        var action = Assert.IsType<CopilotProposedAction>(message.ProposedAction);
        Assert.Equal("pending", message.ActionStatus);
        Assert.Contains(action.Arguments, a => a.Path == "items.0.quantity" && a.Kind == CopilotArgumentKind.Number && a.Value == "2");
        Assert.Contains(action.Arguments, a => a.Path == "items.0.billable" && a.Kind == CopilotArgumentKind.Boolean);
        Assert.Equal("200", action.Details["Totale"]);
        Assert.Equal(["items.0.quantity"], action.EditableFields);
    }

    /// <summary>
    /// A KPI card carries the metric and the value.
    /// </summary>
    [Fact]
    public void ToVisualization_MapsKpiCard()
    {
        using var document = JsonDocument.Parse("""{"type":"kpi_card","title":"Totale","columns":["total"],"rows":[{"total":1234.5}],"metric":"total","value":1234.5}""");
        var visualization = CopilotApiMapper.ToVisualization(document.RootElement);

        Assert.True(visualization.IsKpi);
        Assert.Equal("total", visualization.Metric);
        Assert.Equal("1234.5", visualization.Value);
    }

    /// <summary>
    /// Server-sent events are converted to typed stream events and unknown events are skipped.
    /// </summary>
    [Fact]
    public void ToStreamEvent_MapsKnownEvents()
    {
        var token = CopilotApiMapper.ToStreamEvent(new CopilotSseEvent("token", "{\"text\":\"Ciao\"}"));
        var completed = CopilotApiMapper.ToStreamEvent(new CopilotSseEvent("message.completed", "{\"message_id\":\"m1\",\"conversation_id\":\"c1\"}"));
        var error = CopilotApiMapper.ToStreamEvent(new CopilotSseEvent("error", "{\"message\":\"boom\"}"));
        var unknown = CopilotApiMapper.ToStreamEvent(new CopilotSseEvent("something.else", "{}"));

        Assert.Equal(CopilotStreamEventKind.Token, token?.Kind);
        Assert.Equal("Ciao", token?.Text);
        Assert.Equal("m1", completed?.MessageId);
        Assert.Equal("c1", completed?.ConversationId);
        Assert.Equal("boom", error?.Text);
        Assert.Null(unknown);
    }

    /// <summary>
    /// An action.proposed event may wrap the action in a proposed_action property and carry the message identifier.
    /// </summary>
    [Fact]
    public void ToStreamEvent_UnwrapsProposedAction()
    {
        var proposed = CopilotApiMapper.ToStreamEvent(new CopilotSseEvent("action.proposed", """{"message_id":"m9","proposed_action":{"tool":"t","module":"m","status":"pending","summary":"s","arguments":{"a":1},"details":{},"editable_fields":["a"]}}"""));

        Assert.Equal(CopilotStreamEventKind.ActionProposed, proposed?.Kind);
        Assert.Equal("m9", proposed?.MessageId);
        Assert.Equal("s", proposed?.ProposedAction?.Summary);
    }
}

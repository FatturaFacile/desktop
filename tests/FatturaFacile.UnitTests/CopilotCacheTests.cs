// <copyright file="CopilotCacheTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using FatturaFacile.Application.Copilot;
using FatturaFacile.Application.Features.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the SQLite cache that keeps Copilot history and regulatory snippets viewable offline.
/// </summary>
public sealed class CopilotCacheTests
{
    private static readonly CopilotCitation Regulatory = new("regulatory", "[LIVA Art. 25]", "LIVA", "Art. 25", "CH", null, null, null);

    private static readonly CopilotProposedAction Proposed = new(
        "create_invoice",
        "invoicing",
        "pending",
        "Crea fattura",
        [new CopilotArgument("items.0.quantity", "2", CopilotArgumentKind.Number)],
        new Dictionary<string, string> { ["Cliente"] = "ACME" },
        ["items.0.quantity"],
        null,
        null);

    /// <summary>
    /// A conversation with messages, citations, visualization and action survives a round trip through the cache.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task CacheAndRead_RoundTripsStructuredPayloads()
    {
        using CopilotTestHost host = new();
        var visualization = new CopilotVisualization(
            "bar",
            "Fatturato",
            ["month", "total"],
            [new Dictionary<string, string> { ["month"] = "2026-01", ["total"] = "10" }],
            "month",
            ["2026-01"],
            [new CopilotDataset("total", [10d])],
            null,
            null);

        _ = await host.Sender.Send(new CacheCopilotConversationCommand(Conversation("c1", [
            Message("m1", "user", "Domanda"),
            Message("m2", "assistant", "Risposta [LIVA Art. 25]", [Regulatory], visualization, Proposed, "pending"),
        ])));

        var cached = await host.Sender.Send(new GetCachedCopilotConversationQuery("c1"));

        Assert.NotNull(cached);
        Assert.Equal(2, cached.Messages.Count);
        var answer = cached.Messages[1];
        Assert.Equal("Risposta [LIVA Art. 25]", answer.ContentMarkdown);
        Assert.Equal("LIVA", Assert.Single(answer.Citations).Title);
        Assert.Equal("Art. 25", answer.Citations[0].SectionRef);
        Assert.Equal("bar", answer.Visualization?.Type);
        Assert.Equal("10", answer.Visualization?.Rows[0]["total"]);
        Assert.Equal("pending", answer.ActionStatus);
        Assert.Equal(["items.0.quantity"], answer.ProposedAction?.EditableFields);
    }

    /// <summary>
    /// Caching the same conversation twice updates it instead of duplicating it, and a message update changes the action status.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Cache_IsIdempotentAndMessageUpdateChangesActionStatus()
    {
        using CopilotTestHost host = new();
        var conversation = Conversation("c1", [Message("m2", "assistant", "Risposta", [], null, Proposed, "pending")]);

        _ = await host.Sender.Send(new CacheCopilotConversationCommand(conversation));
        _ = await host.Sender.Send(new CacheCopilotConversationCommand(conversation with { Title = "Nuovo titolo" }));
        var updated = await host.Sender.Send(new CacheCopilotMessageCommand(Message("m2", "assistant", "Risposta", [], null, Proposed with { Result = "Fattura creata" }, "executed")));

        var list = await host.Sender.Send(new GetCachedCopilotConversationsQuery());
        var cached = await host.Sender.Send(new GetCachedCopilotConversationQuery("c1"));

        Assert.True(updated);
        Assert.Equal("Nuovo titolo", Assert.Single(list).Title);
        var message = Assert.Single(cached!.Messages);
        Assert.Equal("executed", message.ActionStatus);
        Assert.Equal("Fattura creata", message.ProposedAction?.Result);
    }

    /// <summary>
    /// The cache of a tenant is invisible to another tenant.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Cache_IsIsolatedPerTenant()
    {
        using CopilotTestHost host = new();
        var first = host.Tenant.TenantId;
        _ = await host.Sender.Send(new CacheCopilotConversationCommand(Conversation("c1", [])));

        host.Tenant.SetCurrentTenant(Guid.NewGuid(), "Other", "IT", "EUR");
        Assert.Empty(await host.Sender.Send(new GetCachedCopilotConversationsQuery()));
        Assert.Null(await host.Sender.Send(new GetCachedCopilotConversationQuery("c1")));

        host.Tenant.SetCurrentTenant(first, "ACME", "CH", "CHF");
        _ = Assert.Single(await host.Sender.Send(new GetCachedCopilotConversationsQuery()));
    }

    /// <summary>
    /// Deleting a conversation removes it and its messages from the cache.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Delete_RemovesConversationAndMessages()
    {
        using CopilotTestHost host = new();
        _ = await host.Sender.Send(new CacheCopilotConversationCommand(Conversation("c1", [Message("m1", "user", "Ciao")])));

        var deleted = await host.Sender.Send(new DeleteCachedCopilotConversationCommand("c1"));

        Assert.True(deleted);
        Assert.Null(await host.Sender.Send(new GetCachedCopilotConversationQuery("c1")));
        Assert.False(await host.Sender.Send(new DeleteCachedCopilotConversationCommand("c1")));
    }

    private static CopilotConversation Conversation(string id, IReadOnlyList<CopilotMessage> messages) =>
        new(id, "Fatture", "it", new CopilotContext("/invoicing", "invoice", "42"), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, messages);

    private static CopilotMessage Message(
        string id,
        string role,
        string text,
        IReadOnlyList<CopilotCitation>? citations = null,
        CopilotVisualization? visualization = null,
        CopilotProposedAction? action = null,
        string? status = null) =>
        new(id, "c1", role, text, citations ?? [], visualization, action, status, DateTimeOffset.UtcNow);
}

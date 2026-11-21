// <copyright file="CopilotSseParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.IO;
using System.Text;
using FatturaFacile.Infrastructure.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the server-sent events reader of the Copilot stream.
/// </summary>
public sealed class CopilotSseParserTests
{
    /// <summary>
    /// Events are split on blank lines, comments are ignored and multiple data lines are joined.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Parse_SplitsEventsAndIgnoresComments()
    {
        const string body = ": keep-alive\n\nevent: message.started\ndata: {}\n\nevent: token\ndata: {\"text\":\"Ciao\"}\n\nevent: error\ndata: line1\ndata: line2\n\n";
        await using MemoryStream stream = new(Encoding.UTF8.GetBytes(body));
        List<CopilotSseEvent> events = [];

        await foreach (var item in CopilotSseParser.ParseAsync(stream))
        {
            events.Add(item);
        }

        Assert.Equal(3, events.Count);
        Assert.Equal("message.started", events[0].Name);
        Assert.Equal("token", events[1].Name);
        Assert.Equal("{\"text\":\"Ciao\"}", events[1].Data);
        Assert.Equal("line1\nline2", events[2].Data);
    }

    /// <summary>
    /// An event that is not terminated by a blank line at the end of the stream is still delivered.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Parse_DeliversTrailingEventWithoutBlankLine()
    {
        await using MemoryStream stream = new(Encoding.UTF8.GetBytes("event: token\r\ndata: {\"text\":\"x\"}"));
        List<CopilotSseEvent> events = [];

        await foreach (var item in CopilotSseParser.ParseAsync(stream))
        {
            events.Add(item);
        }

        _ = Assert.Single(events);
        Assert.Equal("token", events[0].Name);
    }
}

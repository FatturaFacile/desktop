// <copyright file="CopilotSseParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FatturaFacile.Infrastructure.Copilot;

/// <summary>
/// Reads a text/event-stream body into events (event/data fields, comment lines ignored, blank line dispatches).
/// </summary>
public static class CopilotSseParser
{
    /// <summary>
    /// Parses the stream lazily.
    /// </summary>
    /// <param name="stream">The response body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The events in arrival order.</returns>
    public static IAsyncEnumerable<CopilotSseEvent> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return ReadAsync(stream, cancellationToken);
    }

    private static async IAsyncEnumerable<CopilotSseEvent> ReadAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using StreamReader reader = new(stream, Encoding.UTF8);
        var name = string.Empty;
        List<string> data = [];

        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            if (line.Length == 0)
            {
                if (data.Count > 0 || name.Length > 0)
                {
                    yield return new CopilotSseEvent(name.Length > 0 ? name : "message", string.Join('\n', data));
                }

                name = string.Empty;
                data = [];
                continue;
            }

            if (line[0] == ':')
            {
                continue;
            }

            var colon = line.IndexOf(':', StringComparison.Ordinal);
            var field = colon < 0 ? line : line[..colon];
            var value = colon < 0 ? string.Empty : line[(colon + 1)..].TrimStart(' ');

            if (string.Equals(field, "event", StringComparison.Ordinal))
            {
                name = value;
            }
            else if (string.Equals(field, "data", StringComparison.Ordinal))
            {
                data.Add(value);
            }
        }

        if (data.Count > 0 || name.Length > 0)
        {
            yield return new CopilotSseEvent(name.Length > 0 ? name : "message", string.Join('\n', data));
        }
    }
}

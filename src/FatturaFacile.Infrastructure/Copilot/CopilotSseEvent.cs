// <copyright file="CopilotSseEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Copilot;

/// <summary>
/// A raw server-sent event.
/// </summary>
/// <param name="Name">The event name.</param>
/// <param name="Data">The data payload (multiple data lines joined by a line feed).</param>
public sealed record CopilotSseEvent(string Name, string Data);

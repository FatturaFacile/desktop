// <copyright file="CopilotStreamEventKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// The kinds of server-sent events of an assistant turn.
/// </summary>
public enum CopilotStreamEventKind
{
    /// <summary>The turn started.</summary>
    MessageStarted = 0,

    /// <summary>The assistant calls a tool.</summary>
    ToolCall = 1,

    /// <summary>A tool returned.</summary>
    ToolResult = 2,

    /// <summary>A visualization is available.</summary>
    Visualization = 3,

    /// <summary>An action is proposed.</summary>
    ActionProposed = 4,

    /// <summary>A chunk of the answer text.</summary>
    Token = 5,

    /// <summary>The turn completed.</summary>
    MessageCompleted = 6,

    /// <summary>The turn failed.</summary>
    Error = 7,
}

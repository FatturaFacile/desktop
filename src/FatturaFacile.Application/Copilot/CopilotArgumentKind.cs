// <copyright file="CopilotArgumentKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// The JSON kind of an action argument.
/// </summary>
public enum CopilotArgumentKind
{
    /// <summary>A text value.</summary>
    Text = 0,

    /// <summary>A numeric value.</summary>
    Number = 1,

    /// <summary>A boolean value.</summary>
    Boolean = 2,
}

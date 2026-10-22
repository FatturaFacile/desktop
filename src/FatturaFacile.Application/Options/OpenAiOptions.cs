// <copyright file="OpenAiOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Configuration;

/// <summary>
/// Options for standard OpenAI integration.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// Gets the OpenAI API key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the OpenAI model name.
    /// </summary>
    public string Model { get; init; } = string.Empty;
}

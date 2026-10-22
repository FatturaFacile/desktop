// <copyright file="OllamaOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Options;

/// <summary>
/// Options for Ollama AI integration.
/// </summary>
public sealed class OllamaOptions
{

    /// <summary>
    /// Gets the Ollama host URI.
    /// </summary>
    public string Uri { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Ollama model name.
    /// </summary>
    public string Model { get; init; } = string.Empty;

}

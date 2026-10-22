// <copyright file="AiOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Configuration;

/// <summary>
/// Options for AI integration configuration.
/// </summary>
public sealed class AiOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AI";

    /// <summary>
    /// Gets the Azure OpenAI options.
    /// </summary>
    public AzureOpenAiOptions AzureOpenAI { get; init; } = new();

    /// <summary>
    /// Gets the Ollama options.
    /// </summary>
    public OllamaOptions Ollama { get; init; } = new();

    /// <summary>
    /// Gets the OpenAI options.
    /// </summary>
    public OpenAiOptions OpenAI { get; init; } = new();
}

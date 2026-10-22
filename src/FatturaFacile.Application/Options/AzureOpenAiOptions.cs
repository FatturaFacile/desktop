// <copyright file="AzureOpenAiOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Options;

/// <summary>
/// Options for Azure OpenAI integration.
/// </summary>
public sealed class AzureOpenAiOptions
{

    /// <summary>
    /// Gets the Azure OpenAI service endpoint.
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Azure OpenAI access key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the chat deployment name.
    /// </summary>
    public string Deployment { get; init; } = string.Empty;

    /// <summary>
    /// Gets the embeddings deployment name.
    /// </summary>
    public string EmbeddingsDeployment { get; init; } = string.Empty;

}

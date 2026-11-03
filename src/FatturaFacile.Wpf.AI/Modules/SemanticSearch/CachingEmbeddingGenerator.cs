// <copyright file="CachingEmbeddingGenerator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using DevExpress.Xpf.DemoBase.Helpers;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Modules.SemanticSearch;

/// <summary>
/// Caching decorator for an embedding generator, avoiding redundant vector generation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CachingEmbeddingGenerator"/> class decorating the specified generator.
/// </remarks>
/// <param name="generator">The inner embedding generator generating novel vector embeddings.</param>
public class CachingEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> generator) : IEmbeddingGenerator<string, Embedding<float>>
{

    /// <summary>
    /// The inner embedding generator producing novel vector embeddings.
    /// </summary>
    private readonly IEmbeddingGenerator<string, Embedding<float>> generator = generator;

    /// <summary>
    /// In-memory cache storing previously calculated embedding vectors.
    /// </summary>
    private readonly Dictionary<string, Embedding<float>> cache = LoadCache() ?? [with(global::System.StringComparer.Ordinal)];

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var newValues = values.Except(second: cache.Keys, comparer: StringComparer.Ordinal).ToList();
        UsageDetails usageDetails = null;

        if (newValues.Count > 0)
        {
            var newEmbeddings = await generator.GenerateAsync(values: newValues, options: options, cancellationToken: cancellationToken)
                                               .ConfigureAwait(continueOnCapturedContext: false);

            usageDetails = newEmbeddings.Usage;

            for (var i = 0; i < newValues.Count; i++)
            {
                cache[key: newValues[index: i]] = newEmbeddings[index: i];
            }
        }

        var embeddings = new List<Embedding<float>>();

        foreach (var value in values)
        {
            embeddings.Add(item: cache.TryGetValue(key: value, value: out var embedding) ? embedding : null);
        }

        return new GeneratedEmbeddings<Embedding<float>>(embeddings: embeddings)
        {
            Usage = usageDetails,
        };
    }

    /// <inheritdoc />
    public object GetService(
        Type serviceType,
        object serviceKey = null) => generator.GetService(serviceType: serviceType, serviceKey: serviceKey);

    /// <inheritdoc />
    public void Dispose() => generator.Dispose();

    /// <summary>
    /// Loads embedded precomputed vector cache from assembly manifest resources.
    /// </summary>
    /// <returns>A deserialized dictionary of string embeddings, or <c>null</c> if resource is absent.</returns>
    private static Dictionary<string, Embedding<float>> LoadCache()
    {
        var assembly = typeof(CachingEmbeddingGenerator).Assembly;

        var stream = assembly.GetManifestResourceStream(
                                                        name: DemoHelper.GetPath(
                                                                                 path: $"{assembly.GetName().Name}.Data.",
                                                                                 assembly: assembly) + "MovieVectors.json");

        return stream == null ? null : JsonSerializer.Deserialize<Dictionary<string, Embedding<float>>>(utf8Json: stream);
    }

}

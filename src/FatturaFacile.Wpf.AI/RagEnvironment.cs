// <copyright file="RagEnvironment.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Security;
using FatturaFacile.Wpf.AI.Storage;
using FatturaFacile.Wpf.AI.Storage.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace FatturaFacile.Wpf.AI;

/// <summary>
/// Provides retrieval-augmented generation (RAG) vector store environments and search functionality.
/// </summary>
public static class RagEnvironment
{

    /// <summary>
    /// Vector store collection for top-level vectorized documents.
    /// </summary>
    public static VectorStoreCollection<string, VectorizedDocument>? DocsCollection;

    /// <summary>
    /// Vector store collection for chunked document segments.
    /// </summary>
    public static VectorStoreCollection<string, VectorizedDocumentChunk>? ChunksCollection;

    /// <summary>
    /// Uploader component handling document embedding and insertion into the vector store.
    /// </summary>
    public static VectorStoreUploader? Uploader;

    /// <summary>
    /// Active document identifier filters for vector search operations.
    /// </summary>
    public static string[] CurrentFilter = Array.Empty<string>();

    /// <summary>
    /// File path to the local documents directory.
    /// </summary>
    public static readonly string DocumentsPath = System.IO.Path.Combine(path1: AppContext.BaseDirectory, path2: "Documents");

    /// <summary>
    /// Initializes SQLite vector store collections using the provided embedding generator.
    /// </summary>
    /// <param name="embeddingGenerator">The embedding generator instance.</param>
    public static void InitializeVectorStores(IEmbeddingGenerator embeddingGenerator)
    {
        var dbPath  = System.IO.Path.Combine(path1: AppContext.BaseDirectory, path2: "vectorized-docs.db");
        var connStr = $"Data Source={dbPath}";

        var sqlOpts = new SqliteCollectionOptions
                      {
                          EmbeddingGenerator = embeddingGenerator,
                      };

        DocsCollection   = new SqliteCollection<string, VectorizedDocument>(connectionString: connStr, name: "docs", options: sqlOpts);
        ChunksCollection = new SqliteCollection<string, VectorizedDocumentChunk>(connectionString: connStr, name: "chunks", options: sqlOpts);

        if (DocsCollection != null && ChunksCollection != null)
        {
            Uploader = new VectorStoreUploader(docs: DocsCollection, chunks: ChunksCollection);
        }
    }

    /// <summary>
    /// Searches the vectorized chunk store using semantic similarity for the given phrase.
    /// </summary>
    /// <param name="phrase">The search text query or question.</param>
    /// <returns>An XML-formatted string containing top search results.</returns>
    [Description(description: "Searches for information using a phrase or keyword")]
    public static async Task<string> SearchAsync(string phrase)
    {
        if (ChunksCollection == null)
        {
            return string.Empty;
        }

        VectorSearchOptions<VectorizedDocumentChunk>? options = null;

        if (CurrentFilter.Length > 0)
        {
            options = new VectorSearchOptions<VectorizedDocumentChunk>
                      {
                          Filter = chunk => CurrentFilter.Contains(chunk.DocumentId),
                      };
        }

        var results = await ChunksCollection
                           .SearchAsync(searchValue: phrase, top: 10, options: options)
                           .Select(selector: r => $"<result doc=\"{SecurityElement.Escape(str: r.Record.DocumentId)}\" " +
                                                  $"page=\"{SecurityElement.Escape(str: r.Record.Page.ToString())}\">{SecurityElement.Escape(str: r.Record.Content)}</result>")
                           .ToListAsync();

        return $"""
                <search_results>
                {string.Join(separator: Environment.NewLine, values: results)}
                </search_results>
                """;
    }

}

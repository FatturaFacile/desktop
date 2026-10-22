// <copyright file="RagEnvironment.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using FatturaFacile.Storage;
using FatturaFacile.Storage.Models;
using FatturaFacile.Storage.Sources;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace FatturaFacile;

public static class RagEnvironment
{
    public static VectorStoreCollection<string, VectorizedDocument>? DocsCollection;
    public static VectorStoreCollection<string, VectorizedDocumentChunk>? ChunksCollection;
    public static VectorStoreUploader? Uploader;
    public static string[] CurrentFilter = Array.Empty<string>();
    public static readonly string DocumentsPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Documents");

    public static void InitializeVectorStores(IEmbeddingGenerator embeddingGenerator)
    {
        var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "vectorized-docs.db");
        var connStr = $"Data Source={dbPath}";
        var sqlOpts = new Microsoft.SemanticKernel.Connectors.SqliteVec.SqliteCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        DocsCollection = new Microsoft.SemanticKernel.Connectors.SqliteVec.SqliteCollection<string, VectorizedDocument>(connStr, "docs", sqlOpts);
        ChunksCollection = new Microsoft.SemanticKernel.Connectors.SqliteVec.SqliteCollection<string, VectorizedDocumentChunk>(connStr, "chunks", sqlOpts);
        if (DocsCollection != null && ChunksCollection != null)
            Uploader = new VectorStoreUploader(DocsCollection, ChunksCollection);
    }

    [Description("Searches for information using a phrase or keyword")]
    public static async Task<string> SearchAsync(string phrase)
    {
        if (ChunksCollection == null)
            return string.Empty;

        Microsoft.Extensions.VectorData.VectorSearchOptions<VectorizedDocumentChunk>? options = null;
        if (CurrentFilter.Length > 0)
        {
            options = new Microsoft.Extensions.VectorData.VectorSearchOptions<VectorizedDocumentChunk>
            {
                Filter = chunk => CurrentFilter.Contains(chunk.DocumentId)
            };
        }

        var results = await ChunksCollection
            .SearchAsync(phrase, top: 10, options: options)
            .Select(r => $"<result doc=\"{SecurityElement.Escape(r.Record.DocumentId)}\" " +
                $"page=\"{SecurityElement.Escape(r.Record.Page.ToString())}\">{SecurityElement.Escape(r.Record.Content)}</result>")
            .ToListAsync();

        return $"""
            <search_results>
            {string.Join(Environment.NewLine, results)}
            </search_results>
            """;
    }
}

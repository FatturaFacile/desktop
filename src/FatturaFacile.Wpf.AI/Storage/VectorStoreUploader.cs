// <copyright file="VectorStoreUploader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI.Storage.Models;
using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Wpf.AI.Storage;

/// <summary>
/// Coordinates document synchronization, chunk extraction, and vector store uploading.
/// </summary>
public class VectorStoreUploader
{

    /// <summary>
    /// Vector store collection storing document-level records.
    /// </summary>
    private readonly VectorStoreCollection<string, VectorizedDocument> Docs;

    /// <summary>
    /// Vector store collection storing chunk embeddings.
    /// </summary>
    private readonly VectorStoreCollection<string, VectorizedDocumentChunk> Chunks;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorStoreUploader"/> class bound to the target document and chunk collections.
    /// </summary>
    /// <param name="docs">Vector store collection storing document-level records.</param>
    /// <param name="chunks">Vector store collection storing chunk embeddings.</param>
    public VectorStoreUploader(
        VectorStoreCollection<string, VectorizedDocument>      docs,
        VectorStoreCollection<string, VectorizedDocumentChunk> chunks)
    {
        Docs   = docs;
        Chunks = chunks;
    }

    /// <summary>
    /// Synchronizes the vector store against the provided document source, adding modified files and purging stale records.
    /// </summary>
    /// <param name="source">The document source supplying files and chunk extractions.</param>
    public async Task SynchronizeDocumentsAsync(IDocumentsSource source)
    {
        await Chunks.EnsureCollectionExistsAsync();
        await Docs.EnsureCollectionExistsAsync();

        var existing = await Docs.GetAsync(filter: d => true, top: int.MaxValue).ToListAsync();
        var expected = await source.GetDocumentsToVectorizeAsync();

        var existingById = existing.ToDictionary(keySelector: d => d.DocumentId);
        var expectedById = expected.ToDictionary(keySelector: d => d.DocumentId);

        var toVectorize = expected
                         .Where(predicate: doc => !existingById.TryGetValue(key: doc.DocumentId, value: out var ex) ||
                                                  ex.DocumentVersion != doc.DocumentVersion)
                         .ToList();

        var toDelete = existing
                      .Where(predicate: doc => !expectedById.TryGetValue(key: doc.DocumentId, value: out var exp) ||
                                               exp.DocumentVersion != doc.DocumentVersion)
                      .ToList();

        foreach (var doc in toDelete)
        {
            await DeleteChunks(documentId: doc.DocumentId);
            await Docs.DeleteAsync(key: doc.Key);
        }

        foreach (var doc in toVectorize)
        {
            await DeleteChunks(documentId: doc.DocumentId);
            await Docs.UpsertAsync(record: doc);
            var chunks = await source.GetChunksToVectorizeAsync(document: doc);
            await Chunks.UpsertAsync(records: chunks);
        }
    }

    /// <summary>
    /// Retrieves all document-level entries currently indexed in the vector store.
    /// </summary>
    /// <returns>A collection of indexed <see cref="VectorizedDocument"/> items.</returns>
    public async Task<IEnumerable<VectorizedDocument>> GetUploadedDocumentsAsync() =>
        await Docs.GetAsync(filter: d => true, top: int.MaxValue).ToListAsync();

    /// <summary>
    /// Deletes all vectorized chunks associated with the specified document identifier.
    /// </summary>
    /// <param name="documentId">The document identifier whose chunks should be purged.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    private async Task DeleteChunks(string documentId)
    {
        var toDelete = await Chunks.GetAsync(filter: c => c.DocumentId == documentId, top: int.MaxValue).ToListAsync();

        if (toDelete.Count > 0)
        {
            await Chunks.DeleteAsync(keys: toDelete.Select(selector: c => c.Key));
        }
    }

}

// <copyright file="VectorStoreUploader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI.Storage.Models;
using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Wpf.AI.Storage;

/// <summary>
/// Coordinates document synchronization, chunk extraction, and vector store uploading.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VectorStoreUploader"/> class bound to the target document and chunk collections.
/// </remarks>
/// <param name="docs">Vector store collection storing document-level records.</param>
/// <param name="chunks">Vector store collection storing chunk embeddings.</param>
public class VectorStoreUploader(
    VectorStoreCollection<string, VectorizedDocument> docs,
    VectorStoreCollection<string, VectorizedDocumentChunk> chunks)
{

    /// <summary>
    /// Vector store collection storing document-level records.
    /// </summary>
    private readonly VectorStoreCollection<string, VectorizedDocument> docs = docs;

    /// <summary>
    /// Vector store collection storing chunk embeddings.
    /// </summary>
    private readonly VectorStoreCollection<string, VectorizedDocumentChunk> chunks = chunks;

    /// <summary>
    /// Synchronizes the vector store against the provided document source, adding modified files and purging stale records.
    /// </summary>
    /// <param name="source">The document source supplying files and chunk extractions.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task SynchronizeDocumentsAsync(IDocumentsSource source)
    {
        await chunks.EnsureCollectionExistsAsync();
        await docs.EnsureCollectionExistsAsync();

        var existing = await docs.GetAsync(filter: d => true, top: int.MaxValue).ToListAsync();
        var expected = await source.GetDocumentsToVectorizeAsync();

        var existingById = existing.ToDictionary(keySelector: d => d.DocumentId, comparer: StringComparer.Ordinal);
        var expectedById = expected.ToDictionary(keySelector: d => d.DocumentId, comparer: StringComparer.Ordinal);

        var toVectorize = expected
                         .Where(predicate: doc => !existingById.TryGetValue(key: doc.DocumentId, value: out var ex) ||
!string.Equals(ex.DocumentVersion, doc.DocumentVersion, StringComparison.Ordinal))
                         .ToList();

        var toDelete = existing
                      .Where(predicate: doc => !expectedById.TryGetValue(key: doc.DocumentId, value: out var exp) ||
!string.Equals(exp.DocumentVersion, doc.DocumentVersion, StringComparison.Ordinal))
                      .ToList();

        foreach (var doc in toDelete)
        {
            await DeleteChunks(documentId: doc.DocumentId);
            await docs.DeleteAsync(key: doc.Key);
        }

        foreach (var doc in toVectorize)
        {
            await DeleteChunks(documentId: doc.DocumentId);
            await docs.UpsertAsync(record: doc);
            var chunks = await source.GetChunksToVectorizeAsync(document: doc);
            await this.chunks.UpsertAsync(records: chunks);
        }
    }

    /// <summary>
    /// Retrieves all document-level entries currently indexed in the vector store.
    /// </summary>
    /// <returns>A collection of indexed <see cref="VectorizedDocument"/> items.</returns>
    public async Task<IEnumerable<VectorizedDocument>> GetUploadedDocumentsAsync() =>
        await docs.GetAsync(filter: d => true, top: int.MaxValue).ToListAsync();

    /// <summary>
    /// Deletes all vectorized chunks associated with the specified document identifier.
    /// </summary>
    /// <param name="documentId">The document identifier whose chunks should be purged.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    private async Task DeleteChunks(string documentId)
    {
        var toDelete = await chunks.GetAsync(filter: c => c.DocumentId == documentId, top: int.MaxValue).ToListAsync();

        if (toDelete.Count > 0)
        {
            await chunks.DeleteAsync(keys: toDelete.Select(selector: c => c.Key));
        }
    }

}

// <copyright file="VectorStoreUploader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Storage.Models;
using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Storage;

/// <summary>
/// Coordinates document synchronization, chunk extraction, and vector store uploading.
/// </summary>
public class VectorStoreUploader
{
    private readonly VectorStoreCollection<string, VectorizedDocument> Docs;
    private readonly VectorStoreCollection<string, VectorizedDocumentChunk> Chunks;

    public VectorStoreUploader(
        VectorStoreCollection<string, VectorizedDocument> docs,
        VectorStoreCollection<string, VectorizedDocumentChunk> chunks)
    {
        Docs = docs;
        Chunks = chunks;
    }

    public async Task SynchronizeDocumentsAsync(
        IDocumentsSource source)
    {
        await Chunks.EnsureCollectionExistsAsync();
        await Docs.EnsureCollectionExistsAsync();

        var existing = await Docs.GetAsync(d => true, int.MaxValue).ToListAsync();
        var expected = await source.GetDocumentsToVectorizeAsync();

        var existingById = existing.ToDictionary(d => d.DocumentId);
        var expectedById = expected.ToDictionary(d => d.DocumentId);

        var toVectorize = expected
            .Where(doc => !existingById.TryGetValue(doc.DocumentId, out var ex) ||
                          ex.DocumentVersion != doc.DocumentVersion)
            .ToList();

        var toDelete = existing
            .Where(doc => !expectedById.TryGetValue(doc.DocumentId, out var exp) ||
                          exp.DocumentVersion != doc.DocumentVersion)
            .ToList();

        foreach (var doc in toDelete)
        {
            await DeleteChunks(doc.DocumentId);
            await Docs.DeleteAsync(doc.Key);
        }

        foreach (var doc in toVectorize)
        {
            await DeleteChunks(doc.DocumentId);
            await Docs.UpsertAsync(doc);
            var chunks = await source.GetChunksToVectorizeAsync(doc);
            await Chunks.UpsertAsync(chunks);
        }
    }

    public async Task<IEnumerable<VectorizedDocument>> GetUploadedDocumentsAsync() =>
        await Docs.GetAsync(d => true, int.MaxValue).ToListAsync();

    private async Task DeleteChunks(
        string documentId)
    {
        var toDelete = await Chunks.GetAsync(c => c.DocumentId == documentId, int.MaxValue).ToListAsync();
        if (toDelete.Count > 0)
        {
            await Chunks.DeleteAsync(toDelete.Select(c => c.Key));
        }
    }
}

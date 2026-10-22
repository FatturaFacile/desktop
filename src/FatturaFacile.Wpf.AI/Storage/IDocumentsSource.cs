// <copyright file="IDocumentsSource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Storage.Models;

namespace FatturaFacile.Storage;

/// <summary>
/// Source provider supplying documents and segmented chunks for embedding and vectorization.
/// </summary>
public interface IDocumentsSource
{
    /// <summary>
    /// Retrieves all documents available to be vectorized.
    /// </summary>
    /// <returns>A collection of <see cref="VectorizedDocument"/> objects.</returns>
    Task<IEnumerable<VectorizedDocument>> GetDocumentsToVectorizeAsync();

    /// <summary>
    /// Retrieves document chunks belonging to the specified document.
    /// </summary>
    /// <param name="document">The document whose chunks are to be retrieved.</param>
    /// <returns>A collection of <see cref="VectorizedDocumentChunk"/> objects.</returns>
    Task<IEnumerable<VectorizedDocumentChunk>> GetChunksToVectorizeAsync(
        VectorizedDocument document);
}

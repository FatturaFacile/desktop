// <copyright file="VectorizedDocumentChunk.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Wpf.AI.Storage.Models;

/// <summary>
/// Vectorized chunk segment representing a page or section of a document.
/// </summary>
public class VectorizedDocumentChunk
{

    /// <summary>Gets or sets the chunk vector store primary key.</summary>
    [VectorStoreKey]
    public required string Key { get; set; }

    /// <summary>Gets or sets the parent document identifier.</summary>
    [VectorStoreData(IsIndexed = true)]
    public required string DocumentId { get; set; }

    /// <summary>Gets or sets the page number where the chunk originated.</summary>
    [VectorStoreData]
    public int Page { get; set; }

    /// <summary>Gets or sets the text content of the chunk.</summary>
    [VectorStoreData]
    public required string Content { get; set; }

    /// <summary>Gets the content mapped as the vector payload.</summary>
    [VectorStoreVector(dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
    public string Vector => Content;

}

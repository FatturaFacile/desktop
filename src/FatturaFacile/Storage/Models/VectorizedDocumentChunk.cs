// <copyright file="VectorizedDocumentChunk.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Storage.Models;

public class VectorizedDocumentChunk
{
    [VectorStoreKey]
    public required string Key { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string DocumentId { get; set; }

    [VectorStoreData]
    public int Page { get; set; }

    [VectorStoreData]
    public required string Content { get; set; }

    // Set Dimensions according to the selected embedding model
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
    public string Vector => Content;
}

// <copyright file="VectorizedDocument.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Storage.Models;

public class VectorizedDocument
{
    [VectorStoreKey]
    public required string Key { get; set; }

    [VectorStoreData]
    public required string DocumentId { get; set; }

    [VectorStoreData]
    public required string DocumentVersion { get; set; }
}

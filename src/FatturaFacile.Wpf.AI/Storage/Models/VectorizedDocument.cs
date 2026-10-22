// <copyright file="VectorizedDocument.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.VectorData;

namespace FatturaFacile.Wpf.AI.Storage.Models;

/// <summary>
/// Top-level document record stored in the vector database.
/// </summary>
public class VectorizedDocument
{

    /// <summary>Gets or sets the vector store primary key.</summary>
    [VectorStoreKey]
    public required string Key { get; set; }

    /// <summary>Gets or sets the unique document identifier.</summary>
    [VectorStoreData]
    public required string DocumentId { get; set; }

    /// <summary>Gets or sets the version string or hash of the document.</summary>
    [VectorStoreData]
    public required string DocumentVersion { get; set; }

}

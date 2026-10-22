// <copyright file="IDocumentsSource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

using FatturaFacile.Storage.Models;

namespace FatturaFacile.Storage;

public interface IDocumentsSource
{
    Task<IEnumerable<VectorizedDocument>> GetDocumentsToVectorizeAsync();
    Task<IEnumerable<VectorizedDocumentChunk>> GetChunksToVectorizeAsync(VectorizedDocument document);
}

// <copyright file="ISignedDocumentReader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Extracts the document from a CAdES (P7M) envelope.
/// </summary>
public interface ISignedDocumentReader
{
    /// <summary>
    /// Returns the signed content, or the input itself when it is not a signed envelope.
    /// </summary>
    /// <param name="content">The file content (binary or Base64).</param>
    /// <returns>The document.</returns>
    public byte[] Unwrap(ReadOnlyMemory<byte> content);
}

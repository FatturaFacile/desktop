// <copyright file="SdiInboundFile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Invoicing.Sdi;

/// <summary>
/// A file received from the Sistema di Interscambio.
/// </summary>
/// <param name="FileName">The file name.</param>
/// <param name="Content">The content (XML, or signed P7M).</param>
public sealed record SdiInboundFile(string FileName, ReadOnlyMemory<byte> Content);

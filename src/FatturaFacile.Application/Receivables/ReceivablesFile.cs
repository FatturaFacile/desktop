// <copyright file="ReceivablesFile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// A file downloaded from the server.
/// </summary>
/// <param name="FileName">The file name.</param>
/// <param name="Content">The bytes.</param>
public sealed record ReceivablesFile(string FileName, ReadOnlyMemory<byte> Content);

// <copyright file="FieldServicePdf.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A downloaded service report PDF.
/// </summary>
/// <param name="FileName">The file name.</param>
/// <param name="Content">The PDF bytes.</param>
public sealed record FieldServicePdf(string FileName, ReadOnlyMemory<byte> Content);

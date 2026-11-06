// <copyright file="RenderedReport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Reporting;

/// <summary>
/// A generated PDF document.
/// </summary>
/// <param name="Content">The PDF bytes.</param>
/// <param name="FileName">The suggested file name.</param>
/// <param name="DocumentType">The document type.</param>
public sealed record RenderedReport(ReadOnlyMemory<byte> Content, string FileName, ReportDocumentType DocumentType);

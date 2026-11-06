// <copyright file="ReportTemplateInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Reporting;

/// <summary>
/// A report template file found in the template folder of a document type.
/// </summary>
/// <param name="Name">The display name (the file name without extension, e.g. "Fattura_IT").</param>
/// <param name="FilePath">The absolute path of the .docx template.</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="LastModified">The last modification time.</param>
public sealed record ReportTemplateInfo(string Name, string FilePath, ReportDocumentType DocumentType, DateTime LastModified);

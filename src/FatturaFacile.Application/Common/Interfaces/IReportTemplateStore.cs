// <copyright file="IReportTemplateStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// The report templates kept as .docx files on the file system, one folder per document type.
/// Users copy, rename and edit them in Word; the list is read again at every print.
/// </summary>
public interface IReportTemplateStore
{
    /// <summary>
    /// Gets the root folder of the templates.
    /// </summary>
    public string RootDirectory { get; }

    /// <summary>
    /// Returns the folder of a document type, creating it when missing.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <returns>The absolute folder path.</returns>
    public string GetDirectory(ReportDocumentType documentType);

    /// <summary>
    /// Lists the templates currently present in the folder of a document type, sorted by name.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <returns>The templates.</returns>
    public IReadOnlyList<ReportTemplateInfo> GetTemplates(ReportDocumentType documentType);

    /// <summary>
    /// Creates the standard templates of every document type whose folder is empty or missing.
    /// </summary>
    public void EnsureDefaultTemplates();

    /// <summary>
    /// Copies a template under a new name in the same folder.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="sourceName">The name of the template to copy.</param>
    /// <param name="newName">The new name (without extension).</param>
    /// <returns>The new template.</returns>
    public ReportTemplateInfo Duplicate(ReportDocumentType documentType, string sourceName, string newName);
}

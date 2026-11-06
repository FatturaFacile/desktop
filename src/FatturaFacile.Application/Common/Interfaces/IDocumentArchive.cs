// <copyright file="IDocumentArchive.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Reporting;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Stores generated documents in the data folder ("Documents\&lt;company&gt;\&lt;type&gt;"), included in backups.
/// </summary>
public interface IDocumentArchive
{
    /// <summary>
    /// Saves a generated document, replacing a previous version with the same name.
    /// </summary>
    /// <param name="companyName">The company name used as sub-folder.</param>
    /// <param name="documentType">The document type.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="content">The content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The absolute path of the saved file.</returns>
    public Task<string> SaveAsync(string companyName, ReportDocumentType documentType, string fileName, byte[] content, CancellationToken cancellationToken = default);
}

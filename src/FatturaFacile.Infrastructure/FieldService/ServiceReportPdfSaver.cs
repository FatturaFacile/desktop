// <copyright file="ServiceReportPdfSaver.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using FatturaFacile.Application.FieldService;

namespace FatturaFacile.Infrastructure.FieldService;

/// <summary>
/// <see cref="IServiceReportPdfSaver"/> that writes the PDF in `Documents/FatturaFacile/ServiceReports` of the user. Only the file
/// name of the PDF is used, so a hostile name cannot leave that folder.
/// </summary>
public sealed class ServiceReportPdfSaver : IServiceReportPdfSaver
{
    private const string FolderName = "FatturaFacile";
    private const string SubFolderName = "ServiceReports";
    private const string FallbackName = "service-report.pdf";

    /// <inheritdoc/>
    public async Task<string> SaveAsync(FieldServicePdf pdf, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pdf);

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FolderName, SubFolderName);
        _ = Directory.CreateDirectory(folder);

        var name = Path.GetFileName(pdf.FileName);
        var path = Path.Combine(folder, string.IsNullOrWhiteSpace(name) ? FallbackName : name);
        await File.WriteAllBytesAsync(path, pdf.Content.ToArray(), cancellationToken).ConfigureAwait(false);

        return path;
    }
}

// <copyright file="IServiceReportPdfSaver.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Port that puts a downloaded service report PDF on the disk of the user.
/// </summary>
public interface IServiceReportPdfSaver
{
    /// <summary>
    /// Saves the PDF in the service report folder of the user.
    /// </summary>
    /// <param name="pdf">The PDF.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The full path of the saved file.</returns>
    public Task<string> SaveAsync(FieldServicePdf pdf, CancellationToken cancellationToken);
}

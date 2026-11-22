// <copyright file="DownloadServiceReportPdfCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="DownloadServiceReportPdfCommand"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="saver">The file saver.</param>
public sealed class DownloadServiceReportPdfCommandHandler(IFieldServiceApiClient api, IServiceReportPdfSaver saver) : IRequestHandler<DownloadServiceReportPdfCommand, FieldServicePdfResult>
{
    /// <inheritdoc/>
    public async Task<FieldServicePdfResult> Handle(DownloadServiceReportPdfCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!api.IsAuthenticated)
        {
            return new FieldServicePdfResult(FieldServiceCallStatus.Unauthorized, Path: null, Detail: null);
        }

        string? path = null;
        var result = await FieldServiceCallRunner.RunAsync(
            async () =>
            {
                var pdf = await api.DownloadReportPdfAsync(request.ReportId, cancellationToken).ConfigureAwait(false);
                path = await saver.SaveAsync(pdf, cancellationToken).ConfigureAwait(false);

                return new FieldServiceCallResult(FieldServiceCallStatus.Ok);
            },
            cancellationToken).ConfigureAwait(false);

        return new FieldServicePdfResult(result.Status, path, result.Detail);
    }
}

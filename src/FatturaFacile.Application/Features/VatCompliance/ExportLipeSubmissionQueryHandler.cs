// <copyright file="ExportLipeSubmissionQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="ExportLipeSubmissionQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="localization">The localization service.</param>
public sealed class ExportLipeSubmissionQueryHandler(IUnitOfWork unitOfWork, ILocalizationService localization) : IRequestHandler<ExportLipeSubmissionQuery, GeneratedFile>
{
    /// <inheritdoc/>
    public Task<GeneratedFile> Handle(ExportLipeSubmissionQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.SubmissionId, cancellationToken);
    }

    private async Task<GeneratedFile> HandleInternalAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var submission = await unitOfWork.Repository<LipeSubmission>().GetByIdAsync(submissionId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException(VatComplianceErrors.SubmissionNotFound);

        return new GeneratedFile(
            submission.FileName,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(submission.XmlContent),
            string.Format(CultureInfo.CurrentCulture, localization["VatCompliance_Export_LipeSummary"], submission.IsXsdValid ? localization["VatCompliance_Lipe_Valid"] : localization["VatCompliance_Lipe_Invalid"], submission.XmlSha256));
    }
}

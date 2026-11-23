// <copyright file="AttachSignedLipeCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="AttachSignedLipeCommand"/>: the signature of the declarant is read from the file, and the content it
/// carries must be the XML that was generated and validated.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="reader">The reader of signed documents.</param>
/// <param name="time">The clock.</param>
public sealed class AttachSignedLipeCommandHandler(IUnitOfWork unitOfWork, ISignedDocumentReader reader, TimeProvider time) : IRequestHandler<AttachSignedLipeCommand, LipeSubmission>
{
    /// <inheritdoc/>
    public Task<LipeSubmission> Handle(AttachSignedLipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<LipeSubmission> HandleInternalAsync(AttachSignedLipeCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<LipeSubmission>();
        var submission = await repository.GetByIdAsync(request.SubmissionId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException(VatComplianceErrors.SubmissionNotFound);

        if (!submission.IsXsdValid || submission.Status is not (LipeTransmissionStatus.Validated or LipeTransmissionStatus.SignatureVerified))
        {
            throw new InvalidOperationException(VatComplianceErrors.SubmissionState);
        }

        // The reader returns a plain XML file unchanged, so a file whose unwrapped content equals itself carries no signature.
        var content = reader.Unwrap(request.SignedContent);
        if (request.SignedContent.Span.SequenceEqual(content))
        {
            throw new InvalidOperationException(VatComplianceErrors.NotSigned);
        }

        if (!string.Equals(Convert.ToHexStringLower(SHA256.HashData(content)), submission.XmlSha256, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(VatComplianceErrors.SignatureMismatch);
        }

        submission.SignedFileName = request.FileName;
        submission.SignedAtUtc = time.GetUtcNow().UtcDateTime;
        submission.Status = LipeTransmissionStatus.SignatureVerified;
        repository.Update(submission);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return submission;
    }
}

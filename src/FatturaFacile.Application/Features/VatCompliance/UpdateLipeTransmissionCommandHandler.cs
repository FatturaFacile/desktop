// <copyright file="UpdateLipeTransmissionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="UpdateLipeTransmissionCommand"/>: Signed to Transmitted, then Transmitted to Accepted or Rejected. A
/// rejected communication can be prepared again.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="time">The clock.</param>
public sealed class UpdateLipeTransmissionCommandHandler(IUnitOfWork unitOfWork, TimeProvider time) : IRequestHandler<UpdateLipeTransmissionCommand, LipeSubmission>
{
    /// <inheritdoc/>
    public Task<LipeSubmission> Handle(UpdateLipeTransmissionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private static bool IsAllowed(LipeTransmissionStatus from, LipeTransmissionStatus to) => (from, to) switch
    {
        (LipeTransmissionStatus.SignatureVerified, LipeTransmissionStatus.Transmitted) => true,
        (LipeTransmissionStatus.Transmitted, LipeTransmissionStatus.Accepted) => true,
        (LipeTransmissionStatus.Transmitted, LipeTransmissionStatus.Rejected) => true,
        _ => false,
    };

    private async Task<LipeSubmission> HandleInternalAsync(UpdateLipeTransmissionCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<LipeSubmission>();
        var submission = await repository.GetByIdAsync(request.SubmissionId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException(VatComplianceErrors.SubmissionNotFound);

        if (!IsAllowed(submission.Status, request.Status))
        {
            throw new InvalidOperationException(VatComplianceErrors.SubmissionState);
        }

        if (request.Status == LipeTransmissionStatus.Transmitted)
        {
            submission.TransmittedAtUtc = time.GetUtcNow().UtcDateTime;
        }

        submission.Status = request.Status;
        submission.ReceiptProtocol = string.IsNullOrWhiteSpace(request.ReceiptProtocol) ? submission.ReceiptProtocol : request.ReceiptProtocol.Trim();
        submission.ReceiptNote = string.IsNullOrWhiteSpace(request.ReceiptNote) ? submission.ReceiptNote : request.ReceiptNote.Trim();
        repository.Update(submission);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return submission;
    }
}

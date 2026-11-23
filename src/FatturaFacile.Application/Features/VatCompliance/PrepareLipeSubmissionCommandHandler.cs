// <copyright file="PrepareLipeSubmissionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Taxation;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="PrepareLipeSubmissionCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class PrepareLipeSubmissionCommandHandler(IUnitOfWork unitOfWork, ICurrentTenantService tenant) : IRequestHandler<PrepareLipeSubmissionCommand, LipeSubmission>
{
    /// <inheritdoc/>
    public Task<LipeSubmission> Handle(PrepareLipeSubmissionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.VatReturnId, cancellationToken);
    }

    private async Task<LipeSubmission> HandleInternalAsync(Guid vatReturnId, CancellationToken cancellationToken)
    {
        var vatReturn = await unitOfWork.Repository<VatReturn>().GetByIdAsync(vatReturnId, cancellationToken).ConfigureAwait(false);
        if (vatReturn is null || !string.Equals(vatReturn.Country, "IT", StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException(VatComplianceErrors.ReturnNotFound);
        }

        var company = await unitOfWork.Repository<Tenant>().GetByIdAsync(tenant.TenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException(VatComplianceErrors.ReturnNotFound);
        var from = vatReturn.PeriodFrom.Date;
        var end = vatReturn.PeriodTo.Date.AddDays(1);
        var purchases = await unitOfWork.Repository<PurchaseInvoice>().FindAsync(p => p.RegistrationDate >= from && p.RegistrationDate < end, cancellationToken).ConfigureAwait(false);
        var data = LipeDataFactory.Create(vatReturn, company, purchases.Where(static p => string.Equals(p.Status, "Posted", StringComparison.Ordinal)).Sum(static p => p.TotalNet));

        var errors = LipeRules.Check(data).ToList();
        var xml = LipeXmlBuilder.Build(data);
        errors.AddRange(LipeXsdValidator.Validate(xml));

        var repository = unitOfWork.Repository<LipeSubmission>();
        var existing = await repository.FindAsync(s => s.VatReturnId == vatReturnId, cancellationToken).ConfigureAwait(false);
        if (existing.Any(static s => s.Status >= LipeTransmissionStatus.Transmitted))
        {
            throw new InvalidOperationException(VatComplianceErrors.SubmissionState);
        }

        // A communication that was not sent yet is regenerated in place, so the liquidation has one working file.
        var submission = existing.OrderByDescending(static s => s.CreatedAt).FirstOrDefault();
        if (submission is null)
        {
            submission = new LipeSubmission { VatReturnId = vatReturnId };
            await repository.AddAsync(submission, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            repository.Update(submission);
        }

        submission.Year = data.Year;
        submission.Month = data.Month;
        submission.Quarter = data.Quarter;
        submission.FileName = LipeXmlBuilder.FileName(data);
        submission.XmlContent = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetString(xml);
        submission.XmlSha256 = Convert.ToHexStringLower(SHA256.HashData(xml));
        submission.IsXsdValid = errors.Count == 0;
        submission.ValidationErrors = errors.Count == 0 ? null : string.Join('\n', errors);
        submission.Status = errors.Count == 0 ? LipeTransmissionStatus.Validated : LipeTransmissionStatus.Draft;
        submission.SignedFileName = null;
        submission.SignedAtUtc = null;

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return submission;
    }
}

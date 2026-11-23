// <copyright file="UpdateLipeTransmissionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Records the transmission of a LIPE communication and the receipt of the tax authority.
/// </summary>
/// <param name="SubmissionId">The communication.</param>
/// <param name="Status">The new status: Transmitted, Accepted or Rejected.</param>
/// <param name="ReceiptProtocol">The protocol of the receipt.</param>
/// <param name="ReceiptNote">The outcome text of the receipt.</param>
public sealed record UpdateLipeTransmissionCommand(Guid SubmissionId, LipeTransmissionStatus Status, string? ReceiptProtocol, string? ReceiptNote) : IRequest<LipeSubmission>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

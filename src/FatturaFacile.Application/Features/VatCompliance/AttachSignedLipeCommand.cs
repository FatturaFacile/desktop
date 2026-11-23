// <copyright file="AttachSignedLipeCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Attaches the signed copy of a LIPE communication after checking it carries the generated XML.
/// </summary>
/// <param name="SubmissionId">The communication.</param>
/// <param name="FileName">The name of the signed file.</param>
/// <param name="SignedContent">The signed file.</param>
public sealed record AttachSignedLipeCommand(Guid SubmissionId, string FileName, ReadOnlyMemory<byte> SignedContent) : IRequest<LipeSubmission>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

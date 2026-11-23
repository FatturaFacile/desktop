// <copyright file="ExportLipeSubmissionQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Exports the XML of a prepared LIPE communication.
/// </summary>
/// <param name="SubmissionId">The communication.</param>
public sealed record ExportLipeSubmissionQuery(Guid SubmissionId) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}

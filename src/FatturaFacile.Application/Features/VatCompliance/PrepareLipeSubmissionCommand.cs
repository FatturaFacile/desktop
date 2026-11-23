// <copyright file="PrepareLipeSubmissionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Generates the LIPE XML of a VAT liquidation and validates it against the schema profile and the ministerial rules.
/// </summary>
/// <param name="VatReturnId">The liquidation.</param>
public sealed record PrepareLipeSubmissionCommand(Guid VatReturnId) : IRequest<LipeSubmission>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

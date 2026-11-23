// <copyright file="ValidateSuisseTaxXmlCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// CQRS command to serialize a Swiss VAT calculation into eCH-0217 XML and validate against XSD and quadratura.
/// </summary>
/// <param name="Result">Calculated Swiss VAT result.</param>
/// <param name="UidMwst">Company UID-MWST identifier.</param>
/// <param name="CompanyName">Company legal name.</param>
/// <param name="PendingBezugsteuerCount">Number of unreviewed Bezugsteuer entries.</param>
public sealed record ValidateSuisseTaxXmlCommand(
    SwissVatCalculationResult Result,
    string? UidMwst,
    string CompanyName,
    int PendingBezugsteuerCount = 0)
    : IRequest<ValidateSuisseTaxXmlOutcome>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}

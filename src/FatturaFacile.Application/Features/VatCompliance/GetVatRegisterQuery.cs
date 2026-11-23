// <copyright file="GetVatRegisterQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Reads a VAT register of a period with its riepilogo per rate, totals and numbering gaps.
/// </summary>
/// <param name="Kind">The register.</param>
/// <param name="Section">The sezionale, or null for all.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
public sealed record GetVatRegisterQuery(VatRegisterKind Kind, VatRegisterSection? Section, DateTime From, DateTime To) : IRequest<VatRegisterReport>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}

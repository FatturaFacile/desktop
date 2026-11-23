// <copyright file="RegisterVatDocumentsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Registers the issued invoices and the posted purchase invoices of a period in the sales and purchases VAT registers, with progressive numbering per sezionale.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="ProRataPercent">The pro-rata percentage; null to compute it from the previous year.</param>
public sealed record RegisterVatDocumentsCommand(DateTime From, DateTime To, decimal? ProRataPercent) : IRequest<RegisterVatDocumentsResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

// <copyright file="DetectBezugsteuerCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// CQRS command to scan cross-border supplier invoices for Swiss Bezugsteuer (Box 382/383).
/// </summary>
/// <param name="SupplierName">Supplier name.</param>
/// <param name="SupplierCountry">Supplier ISO country code.</param>
/// <param name="InvoiceNumber">Invoice number.</param>
/// <param name="InvoiceDate">Invoice date.</param>
/// <param name="Description">Service description.</param>
/// <param name="NetAmountChf">Net amount in CHF.</param>
/// <param name="ChargedSwissVatAmount">Domestic Swiss VAT charged on invoice.</param>
/// <param name="Method">Reporting method.</param>
public sealed record DetectBezugsteuerCommand(
    string SupplierName,
    string SupplierCountry,
    string InvoiceNumber,
    DateOnly InvoiceDate,
    string Description,
    decimal NetAmountChf,
    decimal ChargedSwissVatAmount,
    SwissVatMethod Method)
    : IRequest<BezugsteuerEntryDto?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}

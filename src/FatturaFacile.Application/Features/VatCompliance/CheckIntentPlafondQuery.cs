// <copyright file="CheckIntentPlafondQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Checks whether an invoice without VAT under art. 8 c. 1 lett. c may be issued to a customer.
/// </summary>
/// <param name="ContactId">The customer.</param>
/// <param name="Date">The date of the invoice.</param>
/// <param name="Amount">The net amount without VAT.</param>
public sealed record CheckIntentPlafondQuery(Guid ContactId, DateTime Date, decimal Amount) : IRequest<PlafondCheckResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}

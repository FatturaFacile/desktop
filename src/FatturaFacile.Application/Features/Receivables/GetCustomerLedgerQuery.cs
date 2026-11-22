// <copyright file="GetCustomerLedgerQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the customer ledger with its running balance.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="Currency">The ISO currency.</param>
public sealed record GetCustomerLedgerQuery(Guid CustomerId, DateOnly? From, DateOnly? To, string? Currency) : IRequest<ReceivablesLoad<CustomerLedger>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}

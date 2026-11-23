// <copyright file="GetCustomerStatementQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the statement of account of a customer for a period.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="Currency">The ISO currency.</param>
public sealed record GetCustomerStatementQuery(Guid CustomerId, DateOnly From, DateOnly To, string? Currency) : IRequest<ReceivablesLoad<CustomerStatement>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}

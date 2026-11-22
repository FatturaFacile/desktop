// <copyright file="GetCustomerLifecycleQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Query for the 360 summary and timeline of a customer, read from the web-api (online only).
/// </summary>
/// <param name="ContactId">The contact (party master) identifier.</param>
/// <param name="Types">The timeline types to include; empty means all.</param>
public sealed record GetCustomerLifecycleQuery(Guid ContactId, IReadOnlyCollection<string> Types) : IRequest<CustomerLifecycleView?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}

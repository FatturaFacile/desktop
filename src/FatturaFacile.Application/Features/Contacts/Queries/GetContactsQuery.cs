// <copyright file="GetContactsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Queries;

/// <summary>
/// Query to retrieve all contacts (clients, suppliers) available in the active tenant context.
/// </summary>
public sealed record GetContactsQuery : IRequest<IReadOnlyList<ContactDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}

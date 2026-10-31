// <copyright file="GetContactsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts;

/// <summary>
/// Query request to retrieve all client, supplier, and customer contacts for the active tenant.
/// </summary>
public record GetContactsQuery : 
    IRequest<IReadOnlyList<ContactDto>>, 
    IQuery<IReadOnlyList<ContactDto>>;

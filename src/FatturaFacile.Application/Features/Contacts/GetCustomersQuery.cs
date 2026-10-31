// <copyright file="GetCustomersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts;

/// <summary>
/// Query request to retrieve contact records projected as customer DTOs for customer management views.
/// </summary>
public record GetCustomersQuery : 
    IRequest<IReadOnlyList<CustomerDto>>, 
    IQuery<IReadOnlyList<CustomerDto>>;

// <copyright file="GetClientsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Query to retrieve client profiles within the current tenant scope, optionally filtered by active status.
/// </summary>
/// <param name="ActiveOnly">A value indicating whether to return only active clients (<see langword="true"/>, default) or all clients.</param>
public record GetClientsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<ClientDto>>;

// <copyright file="GetTireRecordsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Query to retrieve all customer tire sets currently deposited and stored in the warehouse tire hotel.
/// </summary>
public record GetTireRecordsQuery() : IRequest<IReadOnlyList<TireHotelDto>>;

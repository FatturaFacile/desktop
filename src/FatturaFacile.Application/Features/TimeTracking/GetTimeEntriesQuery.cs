// <copyright file="GetTimeEntriesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Represents get time entries query.
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
public record GetTimeEntriesQuery(Guid? EmployeeId = null) : IRequest<IReadOnlyList<TimeEntryDto>>;

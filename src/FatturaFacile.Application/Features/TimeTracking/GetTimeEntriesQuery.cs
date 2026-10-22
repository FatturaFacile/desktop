// <copyright file="GetTimeEntriesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Query to retrieve time tracking and attendance entries, optionally filtered for a designated employee.
/// </summary>
/// <param name="EmployeeId">The optional employee identifier to filter entries; when omitted or <see langword="null"/>, retrieves all tenant entries.</param>
public record GetTimeEntriesQuery(Guid? EmployeeId = null) : IRequest<IReadOnlyList<TimeEntryDto>>;

// <copyright file="GetEmployeesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Query to retrieve active employee personnel for the active tenant.
/// </summary>
public sealed record GetEmployeesQuery : IRequest<IReadOnlyList<HrEmployeeDto>>;

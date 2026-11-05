// <copyright file="CalculatePayrollPreviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Query to calculate and preview a payroll slip for an employee under the tenant's country regulations.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="TargetMonth">The target calendar month.</param>
public sealed record CalculatePayrollPreviewQuery(Guid EmployeeId, DateTime? TargetMonth = null) : IRequest<PayrollCalculationResultDto?>;

// <copyright file="CalculatePayrollPreviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Strategies;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Handler for <see cref="CalculatePayrollPreviewQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="CalculatePayrollPreviewQueryHandler"/> class.
/// </remarks>
/// <param name="employeeRepository">The employee repository.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="strategyFactory">The country strategy factory.</param>
public sealed class CalculatePayrollPreviewQueryHandler(
    IRepository<HrEmployee> employeeRepository,
    ICurrentTenantService currentTenantService,
    ICountryStrategyFactory strategyFactory)
    : IRequestHandler<CalculatePayrollPreviewQuery, PayrollCalculationResultDto?>
{
    /// <inheritdoc/>
    public Task<PayrollCalculationResultDto?> Handle(
        CalculatePayrollPreviewQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(
            request,
            cancellationToken);
    }

    private async Task<PayrollCalculationResultDto?> HandleInternalAsync(
        CalculatePayrollPreviewQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository
            .GetByIdAsync(
                request.EmployeeId,
                cancellationToken)
            .ConfigureAwait(false);

        if (employee is null)
        {
            return null;
        }

        var month = request.TargetMonth ?? DateTime.Today;

        var payrollStrategy = strategyFactory
            .GetPayrollStrategy(
                currentTenantService.CountryCode);

        return payrollStrategy.CalculatePayroll(
            employee,
            month);
    }
}

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
public sealed class CalculatePayrollPreviewQueryHandler : IRequestHandler<CalculatePayrollPreviewQuery, PayrollCalculationResultDto?>
{
    private readonly IRepository<HrEmployee> _employeeRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICountryStrategyFactory _strategyFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculatePayrollPreviewQueryHandler"/> class.
    /// </summary>
    /// <param name="employeeRepository">The employee repository.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="strategyFactory">The country strategy factory.</param>
    public CalculatePayrollPreviewQueryHandler(
        IRepository<HrEmployee> employeeRepository,
        ICurrentTenantService currentTenantService,
        ICountryStrategyFactory strategyFactory)
    {
        _employeeRepository = employeeRepository;
        _currentTenantService = currentTenantService;
        _strategyFactory = strategyFactory;
    }

    /// <inheritdoc/>
    public async Task<PayrollCalculationResultDto?> Handle(CalculatePayrollPreviewQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken).ConfigureAwait(false);
        if (employee == null)
        {
            return null;
        }

        var month = request.TargetMonth ?? DateTime.Today;
        var payrollStrategy = _strategyFactory.GetPayrollStrategy(_currentTenantService.CountryCode);
        return payrollStrategy.CalculatePayroll(employee, month);
    }
}

// <copyright file="CreateEmployeeCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="CreateEmployeeCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateEmployeeCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateEmployeeCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    IMapper mapper) : IRequestHandler<CreateEmployeeCommand, HrEmployeeDto>
{
    /// <inheritdoc/>
    public Task<HrEmployeeDto> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<HrEmployeeDto> HandleInternalAsync(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        HrEmployee employee = new()
        {
            TenantId = currentTenantService.TenantId,
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            SocialSecurityNumber = request.SocialSecurityNumber,
            DepartmentName = request.DepartmentName,
            JobTitle = request.JobTitle,
            EmploymentStatus = request.EmploymentStatus,
            EmploymentType = request.EmploymentType,
            HireDate = request.HireDate,
            BaseAnnualSalary = request.BaseAnnualSalary,
            HourlyCostRate = request.HourlyCostRate,
            Currency = request.Currency ?? currentTenantService.DefaultCurrency,
            FteRatio = request.FteRatio,
        };

        await unitOfWork.Repository<HrEmployee>().AddAsync(employee, cancellationToken)
           .ConfigureAwait(false);

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<HrEmployeeDto>(employee);
    }
}

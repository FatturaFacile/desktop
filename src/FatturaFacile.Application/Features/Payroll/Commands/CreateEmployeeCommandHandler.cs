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
public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, HrEmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEmployeeCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CreateEmployeeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<HrEmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        HrEmployee employee = new()
        {
            TenantId = _currentTenantService.TenantId,
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
            Currency = request.Currency ?? _currentTenantService.DefaultCurrency,
            FteRatio = request.FteRatio,
        };

        await _unitOfWork.Repository<HrEmployee>().AddAsync(employee, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<HrEmployeeDto>(employee);
    }
}

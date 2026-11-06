// <copyright file="GetEmployeesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Handler for <see cref="GetEmployeesQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetEmployeesQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The employee repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetEmployeesQueryHandler(IRepository<HrEmployee> repository, IMapper mapper) : IRequestHandler<GetEmployeesQuery, IReadOnlyList<HrEmployeeDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<HrEmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<HrEmployeeDto> result = repository.Query()
            .Where(e => e.EmploymentStatus == "active")
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ProjectTo<HrEmployeeDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

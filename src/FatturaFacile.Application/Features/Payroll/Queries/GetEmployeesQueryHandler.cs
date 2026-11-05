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
public sealed class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, IReadOnlyList<HrEmployeeDto>>
{
    private readonly IRepository<HrEmployee> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEmployeesQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The employee repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetEmployeesQueryHandler(IRepository<HrEmployee> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<HrEmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<HrEmployeeDto> result = _repository.Query()
            .Where(e => e.EmploymentStatus == "active")
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ProjectTo<HrEmployeeDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

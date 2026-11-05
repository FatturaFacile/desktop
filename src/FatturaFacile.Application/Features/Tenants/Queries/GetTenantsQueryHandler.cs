// <copyright file="GetTenantsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Queries;

/// <summary>
/// Handler for <see cref="GetTenantsQuery"/>.
/// </summary>
public sealed class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly IRepository<Tenant> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTenantsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The tenant repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetTenantsQueryHandler(IRepository<Tenant> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantDto> result = _repository.Query()
            .OrderBy(t => t.Name)
            .ProjectTo<TenantDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

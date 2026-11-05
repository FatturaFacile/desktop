// <copyright file="GetContactsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Queries;

/// <summary>
/// Handler for <see cref="GetContactsQuery"/>.
/// </summary>
public sealed class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, IReadOnlyList<ContactDto>>
{
    private readonly IRepository<Contact> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetContactsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The contacts repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetContactsQueryHandler(IRepository<Contact> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ContactDto> result = _repository.Query()
            .OrderBy(c => c.CompanyName)
            .ThenBy(c => c.LastName)
            .ProjectTo<ContactDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

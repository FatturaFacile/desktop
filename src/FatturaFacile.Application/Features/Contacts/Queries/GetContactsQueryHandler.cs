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
/// <remarks>
/// Initializes a new instance of the <see cref="GetContactsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The contacts repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetContactsQueryHandler(IRepository<Contact> repository, IMapper mapper) : IRequestHandler<GetContactsQuery, IReadOnlyList<ContactDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ContactDto> result = repository.Query()
            .OrderBy(c => c.CompanyName)
            .ThenBy(c => c.LastName)
            .ProjectTo<ContactDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

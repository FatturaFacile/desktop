// <copyright file="GetContactsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts;

/// <summary>
/// Handles queries requesting contact and customer projections from the tenant data store.
/// </summary>
public class GetContactsQueryHandler : 
    IRequestHandler<GetContactsQuery, IReadOnlyList<ContactDto>>,
    IQueryHandler<GetContactsQuery, IReadOnlyList<ContactDto>>,
    IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>,
    IQueryHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetContactsQueryHandler"/> class.
    /// </summary>
    /// <param name="contactRepository">The contact repository dependency.</param>
    /// <param name="mapper">The AutoMapper instance for entity-to-DTO projection.</param>
    public GetContactsQueryHandler(IContactRepository contactRepository, IMapper mapper)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles retrieval of all tenant contacts via MediatR pipeline.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="ContactDto"/> items.</returns>
    public async Task<IReadOnlyList<ContactDto>> Handle(GetContactsQuery query, CancellationToken cancellationToken)
    {
        var contacts = await _contactRepository.GetAllContactsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ContactDto>>(contacts);
    }

    /// <summary>
    /// Handles retrieval of all tenant contacts via application dispatcher.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="ContactDto"/> items.</returns>
    public Task<IReadOnlyList<ContactDto>> HandleAsync(GetContactsQuery query, CancellationToken cancellationToken = default)
    {
        return Handle(query, cancellationToken);
    }

    /// <summary>
    /// Handles retrieval of customer projections via MediatR pipeline.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="CustomerDto"/> items.</returns>
    public async Task<IReadOnlyList<CustomerDto>> Handle(GetCustomersQuery query, CancellationToken cancellationToken)
    {
        var contacts = await _contactRepository.GetAllContactsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CustomerDto>>(contacts);
    }

    /// <summary>
    /// Handles retrieval of customer projections via application dispatcher.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="CustomerDto"/> items.</returns>
    public Task<IReadOnlyList<CustomerDto>> HandleAsync(GetCustomersQuery query, CancellationToken cancellationToken = default)
    {
        return Handle(query, cancellationToken);
    }
}

// <copyright file="GetInvoicesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Queries;

/// <summary>
/// Handler for <see cref="GetInvoicesQuery"/>.
/// </summary>
public sealed class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    private readonly IRepository<Invoice> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInvoicesQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The invoice repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetInvoicesQueryHandler(IRepository<Invoice> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<InvoiceDto> result = _repository.Query()
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.InvoiceNumber)
            .ProjectTo<InvoiceDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

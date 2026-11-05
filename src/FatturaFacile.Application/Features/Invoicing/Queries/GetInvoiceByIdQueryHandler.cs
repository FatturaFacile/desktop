// <copyright file="GetInvoiceByIdQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Queries;

/// <summary>
/// Handler for <see cref="GetInvoiceByIdQuery"/>.
/// </summary>
public sealed class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IRepository<Invoice> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInvoiceByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The invoice repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetInvoiceByIdQueryHandler(IRepository<Invoice> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        return invoice == null ? null : _mapper.Map<InvoiceDto>(invoice);
    }
}

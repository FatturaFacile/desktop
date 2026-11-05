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
/// <remarks>
/// Initializes a new instance of the <see cref="GetInvoicesQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The invoice repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetInvoicesQueryHandler(IRepository<Invoice> repository, IMapper mapper) : IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<InvoiceDto> result = repository.Query()
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.InvoiceNumber)
            .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}

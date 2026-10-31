// <copyright file="GetInvoicesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Handles invoice queries for both detailed and tabular invoice list projections.
/// </summary>
public class GetInvoicesQueryHandler : 
    IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>,
    IQueryHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>,
    IRequestHandler<GetInvoicesListQuery, IReadOnlyList<InvoiceListItemDto>>,
    IQueryHandler<GetInvoicesListQuery, IReadOnlyList<InvoiceListItemDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInvoicesQueryHandler"/> class.
    /// </summary>
    /// <param name="invoiceRepository">The repository used to query invoice records.</param>
    /// <param name="mapper">The AutoMapper instance for DTO mapping.</param>
    public GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository, IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles retrieval of all full invoice DTOs via MediatR pipeline.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="InvoiceDto"/> items.</returns>
    public async Task<IReadOnlyList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetInvoicesWithDetailsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<InvoiceDto>>(invoices);
    }

    /// <summary>
    /// Handles retrieval of all full invoice DTOs via application dispatcher.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="InvoiceDto"/> items.</returns>
    public Task<IReadOnlyList<InvoiceDto>> HandleAsync(GetInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        return Handle(query, cancellationToken);
    }

    /// <summary>
    /// Handles retrieval of summarized invoice list item DTOs via MediatR pipeline.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="InvoiceListItemDto"/> items.</returns>
    public async Task<IReadOnlyList<InvoiceListItemDto>> Handle(GetInvoicesListQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetInvoicesWithDetailsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<InvoiceListItemDto>>(invoices);
    }

    /// <summary>
    /// Handles retrieval of summarized invoice list item DTOs via application dispatcher.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="InvoiceListItemDto"/> items.</returns>
    public Task<IReadOnlyList<InvoiceListItemDto>> HandleAsync(GetInvoicesListQuery query, CancellationToken cancellationToken = default)
    {
        return Handle(query, cancellationToken);
    }
}

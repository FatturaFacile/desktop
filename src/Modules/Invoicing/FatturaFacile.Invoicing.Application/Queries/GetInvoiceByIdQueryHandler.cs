// <copyright file="GetInvoiceByIdQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Repositories;

namespace FatturaFacile.Invoicing.Application.Queries;

/// <summary>
/// Handler for resolving <see cref="GetInvoiceByIdQuery"/> requests.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetInvoiceByIdQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The invoice repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetInvoiceByIdQueryHandler(IInvoiceRepository repository, IMapper mapper) : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    /// <inheritdoc />
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.InvoiceId, cancellationToken);
        return invoice == null || invoice.TenantId != request.TenantId
            ? Result.Failure<InvoiceDto>("Invoice not found.", "INVOICE_NOT_FOUND")
            : Result.Success(_mapper.Map<InvoiceDto>(invoice));
    }
}

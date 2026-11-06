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
/// <remarks>
/// Initializes a new instance of the <see cref="GetInvoiceByIdQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The invoice repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetInvoiceByIdQueryHandler(
    IRepository<Invoice> repository,
    IMapper mapper)
    : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    /// <inheritdoc/>
    public Task<InvoiceDto?> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<InvoiceDto?> HandleInternalAsync(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await repository
           .GetByIdAsync(request.Id, cancellationToken)
           .ConfigureAwait(false);

        return invoice is null
            ? null
            : mapper.Map<InvoiceDto>(invoice);
    }
}

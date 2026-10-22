// <copyright file="GetInvoicesOverviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Handles <see cref="GetInvoicesOverviewQuery"/> by collecting all Italian and Swiss sales billing records in the current tenant scope.
/// </summary>
/// <param name="itRepo">The repository used to query Italian electronic invoices.</param>
/// <param name="chRepo">The repository used to query Swiss QR-bills.</param>
/// <param name="mapper">The mapper used to project sales entities to their respective DTO formats.</param>
public sealed class GetInvoicesOverviewQueryHandler(
    IItalianInvoiceRepository itRepo,
    ISwissQrBillRepository    chRepo,
    IMapper                   mapper) : IRequestHandler<GetInvoicesOverviewQuery, InvoicesOverviewDto>
{

    /// <summary>
    /// The Italian electronic invoice repository.
    /// </summary>
    private readonly IItalianInvoiceRepository itRepo = itRepo;

    /// <summary>
    /// The Swiss QR-bill repository.
    /// </summary>
    private readonly ISwissQrBillRepository chRepo = chRepo;

    /// <summary>
    /// The mapper used to project invoice entities into DTO representations.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Executes the query, retrieving all Italian and Swiss invoices and combining them into an overview model.
    /// </summary>
    /// <param name="request">The query request instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the query execution returning an <see cref="InvoicesOverviewDto"/>.</returns>
    public async Task<InvoicesOverviewDto> Handle(
        GetInvoicesOverviewQuery request,
        CancellationToken        cancellationToken)
    {
        var it = await itRepo.GetAllAsync(cancellationToken: cancellationToken);
        var ch = await chRepo.GetAllAsync(cancellationToken: cancellationToken);

        return new InvoicesOverviewDto(
                                       ItalianInvoices: mapper.Map<IReadOnlyList<ItalianInvoiceDto>>(source: it),
                                       SwissQrBills: mapper.Map<IReadOnlyList<SwissQrBillDto>>(source: ch));
    }

}

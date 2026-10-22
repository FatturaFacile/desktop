// <copyright file="CreateItalianInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;

using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Handles <see cref="CreateItalianInvoiceCommand"/> by initializing an Italian electronic invoice aggregate, computing totals across lines, and storing the invoice.
/// </summary>
/// <param name="repository">The Italian electronic invoice repository.</param>
/// <param name="tenantProvider">The tenant provider supplying the active tenant isolation boundary.</param>
/// <param name="mapper">The mapper used to project the created entity into an <see cref="ItalianInvoiceDto"/>.</param>
public sealed class CreateItalianInvoiceCommandHandler(
    IItalianInvoiceRepository repository,
    ITenantProvider           tenantProvider,
    IMapper                   mapper) : IRequestHandler<CreateItalianInvoiceCommand, ItalianInvoiceDto>
{

    /// <summary>
    /// The repository used to persist Italian electronic invoice aggregates.
    /// </summary>
    private readonly IItalianInvoiceRepository repository = repository;

    /// <summary>
    /// The tenant provider supplying the active tenant isolation boundary.
    /// </summary>
    private readonly ITenantProvider tenantProvider = tenantProvider;

    /// <summary>
    /// The mapper used to project the created entity into an <see cref="ItalianInvoiceDto"/>.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Executes Italian electronic invoice creation, populates billing items, and persists the invoice record.
    /// </summary>
    /// <param name="request">The command containing invoice metadata and item lines.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the creation operation returning the saved <see cref="ItalianInvoiceDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<ItalianInvoiceDto> Handle(
        CreateItalianInvoiceCommand request,
        CancellationToken           cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var invoice = ItalianElectronicInvoice.Create(
                                                      tenantId: tenantProvider.CurrentTenantId,
                                                      documentNumber: request.DocumentNumber,
                                                      documentDate: request.DocumentDate,
                                                      clientId: request.ClientId,
                                                      cedenteName: request.CedenteName,
                                                      cedenteVat: request.CedenteVat,
                                                      cedenteCf: request.CedenteCf,
                                                      cedenteAddress: request.CedenteAddress,
                                                      cessionarioName: request.CessionarioName,
                                                      cessionarioVat: request.CessionarioVat,
                                                      cessionarioCf: request.CessionarioCf,
                                                      sdiCode: request.SdiCode,
                                                      pec: request.Pec,
                                                      cessionarioAddress: request.CessionarioAddress,
                                                      dueDate: request.DueDate);

        foreach (var line in request.Lines)
        {
            invoice.AddLine(
                            description: line.Description,
                            quantity: line.Quantity,
                            unitPrice: line.UnitPrice,
                            vatRate: line.VatRate,
                            natura: line.Natura);
        }

        await repository.AddAsync(
                                  entity: invoice,
                                  cancellationToken: cancellationToken);

        return mapper.Map<ItalianInvoiceDto>(source: invoice);
    }

}

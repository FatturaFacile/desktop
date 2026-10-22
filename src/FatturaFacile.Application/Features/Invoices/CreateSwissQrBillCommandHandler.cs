// <copyright file="CreateSwissQrBillCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Domain.ValueObjects;

using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Handles <see cref="CreateSwissQrBillCommand"/> by creating and validating a Swiss QR-bill domain entity and persisting it.
/// </summary>
/// <param name="repository">The Swiss QR-bill repository.</param>
/// <param name="tenantProvider">The tenant provider supplying the active tenant isolation boundary.</param>
/// <param name="mapper">The mapper used to convert the domain entity into <see cref="SwissQrBillDto"/>.</param>
public sealed class CreateSwissQrBillCommandHandler(
    ISwissQrBillRepository repository,
    ITenantProvider        tenantProvider,
    IMapper                mapper) : IRequestHandler<CreateSwissQrBillCommand, SwissQrBillDto>
{

    /// <summary>
    /// The repository used to persist Swiss QR-bill entities.
    /// </summary>
    private readonly ISwissQrBillRepository repository = repository;

    /// <summary>
    /// The tenant provider supplying the active tenant isolation boundary.
    /// </summary>
    private readonly ITenantProvider tenantProvider = tenantProvider;

    /// <summary>
    /// The mapper used to convert the domain entity into <see cref="SwissQrBillDto"/>.
    /// </summary>
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Validates QR-IBAN and financial monetary values, instantiates a Swiss QR-bill, and saves it to the repository.
    /// </summary>
    /// <param name="request">The command describing the QR-bill payment particulars and debtor/creditor data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the creation operation returning the saved <see cref="SwissQrBillDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<SwissQrBillDto> Handle(
        CreateSwissQrBillCommand request,
        CancellationToken        cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: request);

        var qrIban    = new QrIban(value: request.CreditorQrIban);
        var rawAmount = Money.Chf(amount: request.AmountChf);

        var bill = SwissQrBill.Create(
                                      tenantId: tenantProvider.CurrentTenantId,
                                      billNumber: request.BillNumber,
                                      billDate: request.BillDate,
                                      clientId: request.ClientId,
                                      creditorName: request.CreditorName,
                                      creditorAddress: request.CreditorAddress,
                                      creditorQrIban: qrIban,
                                      debtorName: request.DebtorName,
                                      debtorAddress: request.DebtorAddress,
                                      rawAmount: rawAmount,
                                      referenceNumber: request.ReferenceNumber,
                                      dueDate: request.DueDate,
                                      message: request.Message);

        await repository.AddAsync(
                                  entity: bill,
                                  cancellationToken: cancellationToken);

        return mapper.Map<SwissQrBillDto>(source: bill);
    }

}

// <copyright file="CreateSwissQrBillCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Handles the <see cref="CreateSwissQrBillCommand"/> request to issue a standardized Swiss QR-bill (QR-Rechnung)
/// adhering to SIX Swiss Payment Standards.
/// </summary>
/// <param name="repository">The repository responsible for persisting QR-bills.</param>
/// <param name="tenantProvider">The tenant context provider.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateSwissQrBillCommandHandler(
    ISwissQrBillRepository repository,
    ITenantProvider        tenantProvider,
    IMapper                mapper) : IRequestHandler<CreateSwissQrBillCommand, SwissQrBillDto>
{

    /// <summary>
    /// The repository responsible for persisting <see cref="SwissQrBill"/> entities.
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

        var data = new SwissQrBillData(
            TenantId: tenantProvider.CurrentTenantId,
            BillNumber: request.BillNumber,
            BillDate: request.BillDate,
            ClientId: request.ClientId,
            CreditorName: request.CreditorName,
            CreditorAddress: request.CreditorAddress,
            CreditorQrIban: qrIban,
            DebtorName: request.DebtorName,
            DebtorAddress: request.DebtorAddress,
            RawAmount: rawAmount,
            ReferenceNumber: request.ReferenceNumber,
            DueDate: request.DueDate,
            Message: request.Message);

        var bill = SwissQrBill.Create(data: data);

        await repository.AddAsync(
                                  entity: bill,
                                  cancellationToken: cancellationToken);

        return mapper.Map<SwissQrBillDto>(source: bill);
    }

}

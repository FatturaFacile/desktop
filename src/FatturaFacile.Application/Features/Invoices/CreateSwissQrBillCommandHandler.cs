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
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<CreateSwissQrBillCommand, SwissQrBillDto>
{
    private readonly ISwissQrBillRepository repository = repository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

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
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var qrIban = new QrIban(request.CreditorQrIban);
        var rawAmount = Money.Chf(request.AmountChf);

        var bill = SwissQrBill.Create(
            tenantProvider.CurrentTenantId,
            request.BillNumber,
            request.BillDate,
            request.ClientId,
            request.CreditorName,
            request.CreditorAddress,
            qrIban,
            request.DebtorName,
            request.DebtorAddress,
            rawAmount,
            request.ReferenceNumber,
            request.DueDate,
            request.Message);

        await repository.AddAsync(
            bill,
            cancellationToken);
        return mapper.Map<SwissQrBillDto>(bill);
    }
}

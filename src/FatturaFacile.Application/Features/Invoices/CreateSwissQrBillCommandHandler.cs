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
/// Represents create swiss qr bill command handler.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="mapper">The mapper.</param>
public sealed class CreateSwissQrBillCommandHandler(
    ISwissQrBillRepository repository,
    ITenantProvider tenantProvider,
    IMapper mapper) : IRequestHandler<CreateSwissQrBillCommand, SwissQrBillDto>
{
    private readonly ISwissQrBillRepository repository = repository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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

// <copyright file="GetDashboardOverviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

/// <summary>
/// Represents get dashboard overview query handler.
/// </summary>
/// <param name="clientRepo">The client repo.</param>
/// <param name="itInvoiceRepo">The it invoice repo.</param>
/// <param name="chBillRepo">The ch bill repo.</param>
/// <param name="tireRepo">The tire repo.</param>
public sealed class GetDashboardOverviewQueryHandler(
    IClientRepository clientRepo,
    IItalianInvoiceRepository itInvoiceRepo,
    ISwissQrBillRepository chBillRepo,
    ITireRecordRepository tireRepo) : IRequestHandler<GetDashboardOverviewQuery, DashboardKpiDto>
{
    private readonly IClientRepository clientRepo = clientRepo;

    private readonly IItalianInvoiceRepository itInvoiceRepo = itInvoiceRepo;

    private readonly ISwissQrBillRepository chBillRepo = chBillRepo;

    private readonly ITireRecordRepository tireRepo = tireRepo;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<DashboardKpiDto> Handle(
        GetDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var clients = await clientRepo.GetActiveClientsAsync(cancellationToken);
        var itInvoices = await itInvoiceRepo.GetAllAsync(cancellationToken);
        var chBills = await chBillRepo.GetAllAsync(cancellationToken);
        var tires = await tireRepo.GetAllInStorageAsync(cancellationToken);

        var revEur = itInvoices.Sum(i => i.TotalGross);
        var revChf = chBills.Where(b => b.Amount.Currency == Domain.Enums.Currency.CHF).Sum(b => b.Amount.Amount);

        var pendingEur = itInvoices.Where(i => i.SdiStatus != Domain.Enums.SdiTransmissionStatus.Delivered)
            .Sum(i => i.TotalGross);
        var pendingChf = chBills.Where(b => !b.IsPaid).Sum(b => b.Amount.Amount);

        var vatEur = itInvoices.Sum(i => i.TotalVat);
        var vatChf = decimal.Round(
            revChf * 0.081m,
            2,
            MidpointRounding.AwayFromZero); // Swiss Standard VAT 8.1%

        return new DashboardKpiDto(
            revEur,
            revChf,
            pendingEur,
            pendingChf,
            vatEur,
            vatChf,
            DateTime.UtcNow.AddDays(15),
            clients.Count,
            itInvoices.Count(i => i.SdiStatus == Domain.Enums.SdiTransmissionStatus.Draft),
            tires.Count);
    }
}

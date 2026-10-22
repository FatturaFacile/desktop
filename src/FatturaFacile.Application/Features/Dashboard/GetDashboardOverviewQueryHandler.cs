// <copyright file="GetDashboardOverviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

/// <summary>
/// Handles <see cref="GetDashboardOverviewQuery"/> by synthesizing key financial and operational figures across Italian and Swiss domain boundaries.
/// </summary>
/// <param name="clientRepo">The client repository used to count active customer accounts.</param>
/// <param name="itInvoiceRepo">The Italian invoice repository used to compute EUR revenue and VAT metrics.</param>
/// <param name="chBillRepo">The Swiss QR-bill repository used to compute CHF revenue and pending receivables.</param>
/// <param name="tireRepo">The tire repository used to aggregate automotive warehouse inventory levels.</param>
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
    /// Processes KPI aggregation across domain repositories, calculating revenues, receivables, and tax estimates in EUR and CHF.
    /// </summary>
    /// <param name="request">The dashboard query request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the aggregation operation returning a populated <see cref="DashboardKpiDto"/>.</returns>
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

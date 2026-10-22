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
    IClientRepository         clientRepo,
    IItalianInvoiceRepository itInvoiceRepo,
    ISwissQrBillRepository    chBillRepo,
    ITireRecordRepository     tireRepo) : IRequestHandler<GetDashboardOverviewQuery, DashboardKpiDto>
{

    /// <summary>
    /// The client repository for counting total active clients.
    /// </summary>
    private readonly IClientRepository clientRepo = clientRepo;

    /// <summary>
    /// The Italian electronic invoice repository for revenue and VAT aggregations.
    /// </summary>
    private readonly IItalianInvoiceRepository itInvoiceRepo = itInvoiceRepo;

    /// <summary>
    /// The Swiss QR-bill repository for Swiss Franc receivables aggregation.
    /// </summary>
    private readonly ISwissQrBillRepository chBillRepo = chBillRepo;

    /// <summary>
    /// The tire hotel repository for active seasonal storage counts.
    /// </summary>
    private readonly ITireRecordRepository tireRepo = tireRepo;

    /// <summary>
    /// Processes KPI aggregation across domain repositories, calculating revenues, receivables, and tax estimates in EUR and CHF.
    /// </summary>
    /// <param name="request">The dashboard query request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the aggregation operation returning a populated <see cref="DashboardKpiDto"/>.</returns>
    public async Task<DashboardKpiDto> Handle(
        GetDashboardOverviewQuery request,
        CancellationToken         cancellationToken)
    {
        var clients    = await clientRepo.GetActiveClientsAsync(cancellationToken: cancellationToken);
        var itInvoices = await itInvoiceRepo.GetAllAsync(cancellationToken: cancellationToken);
        var chBills    = await chBillRepo.GetAllAsync(cancellationToken: cancellationToken);
        var tires      = await tireRepo.GetAllInStorageAsync(cancellationToken: cancellationToken);

        var revEur = itInvoices.Sum(selector: i => i.TotalGross);
        var revChf = chBills.Where(predicate: b => b.Amount.Currency == Domain.Enums.Currency.CHF).Sum(selector: b => b.Amount.Amount);

        var pendingEur = itInvoices.Where(predicate: i => i.SdiStatus != Domain.Enums.SdiTransmissionStatus.Delivered)
                                   .Sum(selector: i => i.TotalGross);

        var pendingChf = chBills.Where(predicate: b => !b.IsPaid).Sum(selector: b => b.Amount.Amount);

        var vatEur = itInvoices.Sum(selector: i => i.TotalVat);

        var vatChf = decimal.Round(
                                   d: revChf * 0.081m,
                                   decimals: 2,
                                   mode: MidpointRounding.AwayFromZero); // Swiss Standard VAT 8.1%

        return new DashboardKpiDto(
                                   TotalRevenueEur: revEur,
                                   TotalRevenueChf: revChf,
                                   PendingReceivablesEur: pendingEur,
                                   PendingReceivablesChf: pendingChf,
                                   PendingVatLiabilityEur: vatEur,
                                   PendingVatLiabilityChf: vatChf,
                                   NextTaxDeadline: DateTime.UtcNow.AddDays(value: 15),
                                   ActiveClientsCount: clients.Count,
                                   PendingInvoicesCount: itInvoices.Count(predicate: i => i.SdiStatus == Domain.Enums.SdiTransmissionStatus.Draft),
                                   StoredTireSetsCount: tires.Count);
    }

}

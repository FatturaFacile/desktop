using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

public record GetDashboardOverviewQuery() : IRequest<DashboardKpiDto>;

public sealed class GetDashboardOverviewQueryHandler(
    IClientRepository clientRepo,
    IItalianInvoiceRepository itInvoiceRepo,
    ISwissQrBillRepository chBillRepo,
    ITireRecordRepository tireRepo) : IRequestHandler<GetDashboardOverviewQuery, DashboardKpiDto>
{
    private readonly IClientRepository _clientRepo = clientRepo;
    private readonly IItalianInvoiceRepository _itInvoiceRepo = itInvoiceRepo;
    private readonly ISwissQrBillRepository _chBillRepo = chBillRepo;
    private readonly ITireRecordRepository _tireRepo = tireRepo;

    public async Task<DashboardKpiDto> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var clients = await _clientRepo.GetActiveClientsAsync(cancellationToken);
        var itInvoices = await _itInvoiceRepo.GetAllAsync(cancellationToken);
        var chBills = await _chBillRepo.GetAllAsync(cancellationToken);
        var tires = await _tireRepo.GetAllInStorageAsync(cancellationToken);

        var revEur = itInvoices.Sum(i => i.TotalGross);
        var revChf = chBills.Where(b => b.Amount.Currency == Domain.Enums.Currency.CHF).Sum(b => b.Amount.Amount);

        var pendingEur = itInvoices.Where(i => i.SdiStatus != Domain.Enums.SdiTransmissionStatus.Delivered).Sum(i => i.TotalGross);
        var pendingChf = chBills.Where(b => !b.IsPaid).Sum(b => b.Amount.Amount);

        var vatEur = itInvoices.Sum(i => i.TotalVat);
        var vatChf = decimal.Round(revChf * 0.081m, 2, MidpointRounding.AwayFromZero); // Swiss Standard VAT 8.1%

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

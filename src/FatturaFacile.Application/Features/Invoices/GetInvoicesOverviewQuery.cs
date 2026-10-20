using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

public record GetInvoicesOverviewQuery() : IRequest<InvoicesOverviewDto>;
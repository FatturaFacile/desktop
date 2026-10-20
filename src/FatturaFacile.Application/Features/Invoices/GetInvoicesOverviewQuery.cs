using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents get invoices overview query.
/// </summary>
public record GetInvoicesOverviewQuery() : IRequest<InvoicesOverviewDto>;

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

public record GetDashboardOverviewQuery() : IRequest<DashboardKpiDto>;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

/// <summary>
/// Represents get dashboard overview query.
/// </summary>
public record GetDashboardOverviewQuery() : IRequest<DashboardKpiDto>;

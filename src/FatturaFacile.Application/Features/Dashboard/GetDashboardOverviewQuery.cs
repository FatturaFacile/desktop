// <copyright file="GetDashboardOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

/// <summary>
/// Query to aggregate multi-currency financial revenues, accounts receivable, VAT liabilities, and operational metrics for executive dashboards.
/// </summary>
public record GetDashboardOverviewQuery() : IRequest<DashboardKpiDto>;

// <copyright file="GetDashboardOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Dashboard;

/// <summary>
/// Represents get dashboard overview query.
/// </summary>
public record GetDashboardOverviewQuery() : IRequest<DashboardKpiDto>;

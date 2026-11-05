// <copyright file="GetChartOfAccountsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Query to retrieve the chart of accounts (shared global and tenant-specific accounts).
/// </summary>
public sealed record GetChartOfAccountsQuery : IRequest<IReadOnlyList<ChartOfAccountDto>>;

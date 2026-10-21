// <copyright file="GetInvoicesOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents get invoices overview query.
/// </summary>
public record GetInvoicesOverviewQuery() : IRequest<InvoicesOverviewDto>;

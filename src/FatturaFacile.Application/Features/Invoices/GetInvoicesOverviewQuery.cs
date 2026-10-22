// <copyright file="GetInvoicesOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Query to retrieve a unified overview containing both Italian electronic invoices and Swiss QR-bills.
/// </summary>
public record GetInvoicesOverviewQuery() : IRequest<InvoicesOverviewDto>;

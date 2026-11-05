// <copyright file="GetInvoicesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Queries;

/// <summary>
/// Query to retrieve all invoices for the current active tenant.
/// </summary>
public sealed record GetInvoicesQuery : IRequest<IReadOnlyList<InvoiceDto>>;

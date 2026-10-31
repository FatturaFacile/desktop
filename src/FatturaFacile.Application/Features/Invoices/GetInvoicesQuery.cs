// <copyright file="GetInvoicesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Query request to retrieve all full invoice documents along with line items and customer records for the active tenant.
/// </summary>
public record GetInvoicesQuery : IRequest<IReadOnlyList<InvoiceDto>>, IQuery<IReadOnlyList<InvoiceDto>>;

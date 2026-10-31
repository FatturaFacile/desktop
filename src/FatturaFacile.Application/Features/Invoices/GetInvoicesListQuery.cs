// <copyright file="GetInvoicesListQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Query request to retrieve lightweight summarized invoice records optimized for grid and list display views.
/// </summary>
public record GetInvoicesListQuery : IRequest<IReadOnlyList<InvoiceListItemDto>>, IQuery<IReadOnlyList<InvoiceListItemDto>>;

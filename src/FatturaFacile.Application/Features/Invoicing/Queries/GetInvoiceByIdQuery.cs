// <copyright file="GetInvoiceByIdQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Queries;

/// <summary>
/// Query to retrieve an invoice by unique identifier.
/// </summary>
/// <param name="Id">The invoice unique identifier.</param>
public sealed record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto?>;

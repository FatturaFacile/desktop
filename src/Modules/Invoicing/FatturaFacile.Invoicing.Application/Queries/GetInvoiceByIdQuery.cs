// <copyright file="GetInvoiceByIdQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Invoicing.Application.DTOs;

namespace FatturaFacile.Invoicing.Application.Queries;

/// <summary>
/// Query to retrieve a detailed invoice projection by its unique identifier and tenant scope.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="InvoiceId">The invoice primary key identifier.</param>
public sealed record GetInvoiceByIdQuery(Guid TenantId, Guid InvoiceId) : IQuery<InvoiceDto>;

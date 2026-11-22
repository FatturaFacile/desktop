// <copyright file="ExportCustomerStatementCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Downloads the statement of a customer as PDF or Excel and saves it on disk.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Format">pdf or xlsx.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="Currency">The ISO currency.</param>
public sealed record ExportCustomerStatementCommand(Guid CustomerId, string Format, DateOnly From, DateOnly To, string? Currency) : IRequest<ReceivablesExportResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}

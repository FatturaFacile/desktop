// <copyright file="ConvertServiceReportToInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Creates the draft invoice of the chargeable part of a signed service report (covered lines are not billed). Needs a connection.
/// </summary>
/// <param name="ReportId">The service report identifier.</param>
/// <param name="TaxRate">The VAT rate in percent.</param>
/// <param name="DueDate">The due date, or null.</param>
public sealed record ConvertServiceReportToInvoiceCommand(string ReportId, decimal TaxRate, DateOnly? DueDate) : IRequest<FieldServiceCallResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}

// <copyright file="SavePayrollRecordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Creates or updates a payroll master data record (employee, child, absence, input, wage type, settings, rates).
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Record">The record values (a new identifier creates the record).</param>
public sealed record SavePayrollRecordCommand<TEntity>(TEntity Record) : IRequest<Guid>, IAuthorizedRequest
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}

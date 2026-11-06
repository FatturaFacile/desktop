// <copyright file="DeletePayrollRecordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Deletes a payroll master data record.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Id">The record identifier.</param>
public sealed record DeletePayrollRecordCommand<TEntity>(Guid Id) : IRequest<bool>, IAuthorizedRequest
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}

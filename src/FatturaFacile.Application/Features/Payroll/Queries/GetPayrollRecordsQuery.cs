// <copyright file="GetPayrollRecordsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Returns detached copies of payroll records of the current company, optionally filtered.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="Filter">The optional filter.</param>
public sealed record GetPayrollRecordsQuery<TEntity>(Expression<Func<TEntity, bool>>? Filter = null)
    : IRequest<IReadOnlyList<TEntity>>, IAuthorizedRequest
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollRead;
}
